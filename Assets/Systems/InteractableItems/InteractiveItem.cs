using System;
using System.Collections.Generic;
using cherrydev;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class InteractiveItem : MonoBehaviour, IInteractable
{
    [Header("Debug")]
    [Tooltip("Enable debug logs (Editor only)")]
    public bool enableDebugLogs = false;
    
    [Header("Dialog Settings")]
    public DialogBehaviour dialogBehaviour;
    public DialogNodeGraph dialogGraph;

    [Header("Interaction Settings")]
    [Tooltip("How far away the player can be to interact with this item")]
    [SerializeField] private float interactionRange = 1f;

    [Header("Flags to Set After Dialog")]
    [Tooltip("These flags will be set when the dialog finishes (e.g., 'talked_to_npc', 'quest_completed')")]
    [SerializeField] private List<string> flagsToSet = new List<string>();

    [Header("Events")]
    [Tooltip("Invoked when the dialog completes - use this to trigger custom scripts or actions")]
    public UnityEvent OnDialogCompleted;

    [Header("Conversation Audio")]
    [Tooltip("Music/audio to play during the conversation with this NPC")]
    [SerializeField] private AudioClip conversationMusic;
    [Tooltip("Volume for the conversation music (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;
    [Tooltip("How long to fade in/out the music")]
    [SerializeField] private float musicFadeDuration = 0.5f;
    [Tooltip("Should the music loop during the conversation?")]
    [SerializeField] private bool loopMusic = true;
    [Tooltip("Volume to reduce room audio to during conversation (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float roomAudioDuckVolume = 0f;
    [Tooltip("How long to fade room audio in/out")]
    [SerializeField] private float roomAudioFadeDuration = 1f;

    // Runtime
    private GameObject player;
    private CharacterMotor2D characterController;
    private ClockTimer clockTimer;
    private bool isPlayerNear = false;
    
    // Track if THIS item started the current conversation
    private bool isMyConversation = false;
    
    // Audio
    private AudioSource conversationAudioSource;
    private Coroutine fadeCoroutine;
    private Coroutine roomAudioFadeCoroutine;
    private RoomAudioZone[] roomAudioZones;
    private float[] originalRoomVolumes;

    void Start()
    {
        // Runtime validation for collider
        if (GetComponent<Collider2D>() == null)
        {
            DebugLogger.LogWarning($"[InteractiveItem] {name} is missing a Collider2D component! " +
                           "Add a BoxCollider2D, CircleCollider2D, or other 2D collider for player interaction to work.");
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            characterController = player.GetComponent<CharacterMotor2D>();

        // Find ClockTimer in the scene
        clockTimer = FindObjectOfType<ClockTimer>();
        if (clockTimer == null)
            DebugLogger.LogWarning($"[InteractiveItem] {name}: No ClockTimer found in scene. Timer pause will not work.");

        if (dialogBehaviour != null)
        {
            dialogBehaviour.OnDialogStarted.AddListener(OnDialogStart);
            dialogBehaviour.OnDialogFinished.AddListener(OnDialogFinished);
        }

        // Setup audio source for conversation music
        if (conversationMusic != null)
        {
            conversationAudioSource = gameObject.AddComponent<AudioSource>();
            conversationAudioSource.clip = conversationMusic;
            conversationAudioSource.loop = loopMusic;
            conversationAudioSource.playOnAwake = false;
            conversationAudioSource.volume = 0f;
            conversationAudioSource.spatialBlend = 0f; // 2D audio
            DebugLogger.LogInteractiveItem($"Audio source created with clip '{conversationMusic.name}'", name);
        }
        else
        {
            DebugLogger.LogInteractiveItem("No conversation music assigned", name);
        }

        // Find all room audio zones for ducking
        roomAudioZones = FindObjectsOfType<RoomAudioZone>();
        originalRoomVolumes = new float[roomAudioZones.Length];
        for (int i = 0; i < roomAudioZones.Length; i++)
        {
            if (roomAudioZones[i].roomMusic != null)
                originalRoomVolumes[i] = roomAudioZones[i].roomMusic.volume;
        }
    }

    void Update()
    {
        if (player == null) return;

        isPlayerNear = Vector3.Distance(transform.position, player.transform.position) <= interactionRange;

        // Note: Individual input checking removed - now handled by InteractionDetector
        // This prevents duplicate E key checks and ensures proper priority ordering
    }

    public void Interact()
    {
        if (!dialogBehaviour)
        {
            DebugLogger.LogWarning($"{name}: Missing DialogBehaviour reference.");
            return;
        }

        if (dialogGraph == null)
        {
            DebugLogger.LogWarning($"{name}: Missing DialogGraph reference.");
            return;
        }

        // Try to acquire the lock
        if (!Systems.InteractionLockManager.TryLock())
        {
            return; // Another interaction is in progress
        }

        // Mark that THIS item is starting the conversation
        isMyConversation = true;
        dialogBehaviour.StartDialog(dialogGraph);
    }

    public int GetInteractionPriority()
    {
        // Dialog interactions have third priority (after teleports and dialog triggers)
        return 2;
    }

    public bool CanInteract()
    {
        // Can interact if we have dialog setup, player is near, and no other interaction is in progress
        if (dialogBehaviour == null || dialogGraph == null) return false;
        if (!isPlayerNear) return false;
        if (Systems.InteractionLockManager.IsLocked) return false;
        if (characterController != null && characterController.IsDialogueActive) return false;
        return true;
    }

    public bool ShowInteractionPrompt()
    {
        // Interactive items DO show the popup icon
        return true;
    }

    void OnDialogStart()
    {
        // Only respond if THIS item started the conversation
        if (!isMyConversation) 
        {
            DebugLogger.LogInteractiveItem("OnDialogStart called but not my conversation - ignoring", name);
            return;
        }

        DebugLogger.LogInteractiveItem("=== DIALOG START ===", name);

        // Pause NPCs and timer via GlobalPause (but not player input or timescale)
        GlobalPause.SetMinigamePaused(true);
        DebugLogger.LogInteractiveItem("GlobalPause minigame pause enabled (NPCs and timer paused)", name);

        if (characterController != null)
            characterController.SetDialogueActive(true);

        // Start conversation music
        if (conversationAudioSource != null && conversationMusic != null)
        {
            DebugLogger.LogInteractiveItem($"Starting conversation music '{conversationMusic.name}' - Current volume: {conversationAudioSource.volume}, Is playing: {conversationAudioSource.isPlaying}", name);
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeMusic(true));
        }
        else
        {
            DebugLogger.LogInteractiveItem($"Cannot start music - AudioSource: {(conversationAudioSource != null ? "OK" : "NULL")}, Clip: {(conversationMusic != null ? "OK" : "NULL")}", name);
        }

        // Duck room audio
        DebugLogger.LogInteractiveItem("Ducking room audio...", name);
        if (roomAudioFadeCoroutine != null)
            StopCoroutine(roomAudioFadeCoroutine);
        roomAudioFadeCoroutine = StartCoroutine(FadeRoomAudio(true));
    }

    void OnDialogFinished()
    {
        // Only respond if THIS item started the conversation
        if (!isMyConversation) return;

        // Resume NPCs and timer via GlobalPause
        GlobalPause.SetMinigamePaused(false);
        DebugLogger.LogInteractiveItem("GlobalPause minigame pause disabled (NPCs and timer resumed)", name);

        // Set all flags when dialog finishes
        if (flagsToSet != null && flagsToSet.Count > 0)
        {
            foreach (string flag in flagsToSet)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    GameFlags.SetFlag(flag);
                    DebugLogger.LogInteractiveItem($"Set flag '{flag}'", name);
                }
            }
        }
        else
        {
            DebugLogger.LogInteractiveItem("No flags to set (flagsToSet is empty)", name);
        }

        if (characterController != null)
            characterController.SetDialogueActive(false);

        // Stop conversation music
        if (conversationAudioSource != null && conversationAudioSource.isPlaying)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeMusic(false));
            DebugLogger.LogInteractiveItem("Stopping conversation music", name);
        }

        // Restore room audio
        if (roomAudioFadeCoroutine != null)
            StopCoroutine(roomAudioFadeCoroutine);
        roomAudioFadeCoroutine = StartCoroutine(FadeRoomAudio(false));

        // Invoke the custom callback
        OnDialogCompleted?.Invoke();

        // Reset the flag so we don't respond to other conversations
        isMyConversation = false;
        
        // Release the interaction lock
        Systems.InteractionLockManager.Unlock();
    }

    private System.Collections.IEnumerator FadeMusic(bool fadeIn)
    {
        if (conversationAudioSource == null) 
        {
            DebugLogger.LogInteractiveItem("FadeMusic: conversationAudioSource is NULL!", name);
            yield break;
        }

        float startVolume = conversationAudioSource.volume;
        float targetVolume = fadeIn ? musicVolume : 0f;
        float elapsed = 0f;

        DebugLogger.LogInteractiveItem($"FadeMusic: {(fadeIn ? "Fading IN" : "Fading OUT")} from {startVolume} to {targetVolume} over {musicFadeDuration}s", name);

        if (fadeIn && !conversationAudioSource.isPlaying)
        {
            conversationAudioSource.Play();
            DebugLogger.LogInteractiveItem($"FadeMusic: Started playing audio clip '{conversationMusic.name}'", name);
        }

        while (elapsed < musicFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / musicFadeDuration;
            conversationAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        conversationAudioSource.volume = targetVolume;
        DebugLogger.LogInteractiveItem($"FadeMusic: Fade complete. Final volume = {conversationAudioSource.volume}, Is playing: {conversationAudioSource.isPlaying}", name);

        if (!fadeIn)
        {
            conversationAudioSource.Stop();
            DebugLogger.LogInteractiveItem("FadeMusic: Stopped audio playback", name);
        }

        fadeCoroutine = null;
    }

    private System.Collections.IEnumerator FadeRoomAudio(bool duck)
    {
        if (roomAudioZones == null || roomAudioZones.Length == 0) 
        {
            DebugLogger.LogInteractiveItem($"No room audio zones found to {(duck ? "duck" : "restore")}", name);
            yield break;
        }

        DebugLogger.LogInteractiveItem($"Found {roomAudioZones.Length} room audio zones to {(duck ? "duck" : "restore")}", name);

        float elapsed = 0f;

        // Store current volumes as starting point
        float[] startVolumes = new float[roomAudioZones.Length];
        for (int i = 0; i < roomAudioZones.Length; i++)
        {
            if (roomAudioZones[i] != null && roomAudioZones[i].roomMusic != null)
            {
                startVolumes[i] = roomAudioZones[i].roomMusic.volume;
                DebugLogger.LogInteractiveItem($"  Zone {i} '{roomAudioZones[i].name}': Current volume = {startVolumes[i]}, Target = {(duck ? roomAudioDuckVolume : originalRoomVolumes[i])}", name);
            }
        }

        while (elapsed < roomAudioFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / roomAudioFadeDuration;

            for (int i = 0; i < roomAudioZones.Length; i++)
            {
                if (roomAudioZones[i] != null && roomAudioZones[i].roomMusic != null)
                {
                    float targetVolume = duck ? roomAudioDuckVolume : originalRoomVolumes[i];
                    roomAudioZones[i].roomMusic.volume = Mathf.Lerp(startVolumes[i], targetVolume, t);
                }
            }
            yield return null;
        }

        // Ensure final volumes are set
        for (int i = 0; i < roomAudioZones.Length; i++)
        {
            if (roomAudioZones[i] != null && roomAudioZones[i].roomMusic != null)
            {
                float targetVolume = duck ? roomAudioDuckVolume : originalRoomVolumes[i];
                roomAudioZones[i].roomMusic.volume = targetVolume;
                
                // If ducking to 0 or near 0, also pause the audio to save CPU
                if (duck && roomAudioDuckVolume < 0.01f)
                {
                    if (roomAudioZones[i].roomMusic.isPlaying)
                    {
                        roomAudioZones[i].roomMusic.Pause();
                        DebugLogger.LogInteractiveItem($"  Paused room audio zone '{roomAudioZones[i].name}'", name);
                    }
                }
                else if (!duck)
                {
                    // Resume if it was paused
                    if (!roomAudioZones[i].roomMusic.isPlaying)
                    {
                        roomAudioZones[i].roomMusic.UnPause();
                        DebugLogger.LogInteractiveItem($"  Unpaused room audio zone '{roomAudioZones[i].name}'", name);
                    }
                }
            }
        }

        DebugLogger.LogInteractiveItem($"Room audio {(duck ? "ducked to " + roomAudioDuckVolume : "restored")}", name);
        roomAudioFadeCoroutine = null;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
#endif
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
            DebugLogger.LogInteractiveItem(message, name);
    }
}


/// <summary>
/// Simple attribute so the field is visible but not editable in the Inspector (runtime-safe).
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
// Editor-only drawer so fields marked [ReadOnly] appear disabled in Inspector.
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool prev = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = prev;
    }
}
#endif
