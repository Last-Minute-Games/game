using System;
using System.Collections.Generic;
using cherrydev;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Removed RequireComponent to allow flexible setup
// Runtime check will warn if collider is missing
public class InteractiveItem : MonoBehaviour, IInteractable
{
    [Header("Dialog Settings")]
    [SerializeField] private DialogBehaviour dialogBehaviour;
    [SerializeField] private DialogNodeGraph dialogGraph;

    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 1f;

    [Header("Flags to Set After Dialog")]
    [Tooltip("These flags will be set when the dialog finishes (e.g., 'talked_to_npc', 'quest_completed')")]
    [SerializeField] private List<string> flagsToSet = new List<string>();

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
            Debug.LogWarning($"[InteractiveItem] {name} is missing a Collider2D component! " +
                           "Add a BoxCollider2D, CircleCollider2D, or other 2D collider for player interaction to work.");
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            characterController = player.GetComponent<CharacterMotor2D>();

        // Find ClockTimer in the scene
        clockTimer = FindObjectOfType<ClockTimer>();
        if (clockTimer == null)
            Debug.LogWarning($"[InteractiveItem] {name}: No ClockTimer found in scene. Timer pause will not work.");

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
            Debug.Log($"[InteractiveItem] {name}: Audio source created with clip '{conversationMusic.name}'");
        }
        else
        {
            Debug.Log($"[InteractiveItem] {name}: No conversation music assigned");
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

        if (isPlayerNear && Input.GetKeyDown(interactKey))
        {
            if (characterController != null && characterController.IsDialogueActive) return;
            Interact();
        }
    }

    public void Interact()
    {
        if (!dialogBehaviour)
        {
            Debug.LogWarning($"{name}: Missing DialogBehaviour reference.");
            return;
        }

        if (dialogGraph == null)
        {
            Debug.LogWarning($"{name}: Missing DialogGraph reference.");
            return;
        }

        // Mark that THIS item is starting the conversation
        isMyConversation = true;
        dialogBehaviour.StartDialog(dialogGraph);
    }

    void OnDialogStart()
    {
        // Only respond if THIS item started the conversation
        if (!isMyConversation) return;

        // Pause the clock timer
        if (clockTimer != null)
        {
            clockTimer.PauseTimer(true);
            Debug.Log($"[InteractiveItem] {name}: Clock timer paused");
        }

        if (characterController != null)
            characterController.SetDialogueActive(true);

        // Start conversation music
        if (conversationAudioSource != null && conversationMusic != null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeMusic(true));
            Debug.Log($"[InteractiveItem] {name}: Starting conversation music '{conversationMusic.name}'");
        }
        else
        {
            Debug.Log($"[InteractiveItem] {name}: Cannot start music - AudioSource: {(conversationAudioSource != null ? "OK" : "NULL")}, Clip: {(conversationMusic != null ? "OK" : "NULL")}");
        }

        // Duck room audio
        if (roomAudioFadeCoroutine != null)
            StopCoroutine(roomAudioFadeCoroutine);
        roomAudioFadeCoroutine = StartCoroutine(FadeRoomAudio(true));
    }

    void OnDialogFinished()
    {
        // Only respond if THIS item started the conversation
        if (!isMyConversation) return;

        // Resume the clock timer
        if (clockTimer != null)
        {
            clockTimer.PauseTimer(false);
            Debug.Log($"[InteractiveItem] {name}: Clock timer resumed");
        }

        // Set all flags when dialog finishes
        if (flagsToSet != null && flagsToSet.Count > 0)
        {
            foreach (string flag in flagsToSet)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    GameFlags.SetFlag(flag);
                    Debug.Log($"[InteractiveItem] {name}: Set flag '{flag}'");
                }
            }
        }
        else
        {
            Debug.Log($"[InteractiveItem] {name}: No flags to set (flagsToSet is empty)");
        }

        if (characterController != null)
            characterController.SetDialogueActive(false);

        // Stop conversation music
        if (conversationAudioSource != null && conversationAudioSource.isPlaying)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeMusic(false));
            Debug.Log($"[InteractiveItem] {name}: Stopping conversation music");
        }

        // Restore room audio
        if (roomAudioFadeCoroutine != null)
            StopCoroutine(roomAudioFadeCoroutine);
        roomAudioFadeCoroutine = StartCoroutine(FadeRoomAudio(false));

        // Reset the flag so we don't respond to other conversations
        isMyConversation = false;
    }

    private System.Collections.IEnumerator FadeMusic(bool fadeIn)
    {
        if (conversationAudioSource == null) yield break;

        float startVolume = conversationAudioSource.volume;
        float targetVolume = fadeIn ? musicVolume : 0f;
        float elapsed = 0f;

        if (fadeIn && !conversationAudioSource.isPlaying)
        {
            conversationAudioSource.Play();
        }

        while (elapsed < musicFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / musicFadeDuration;
            conversationAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        conversationAudioSource.volume = targetVolume;

        if (!fadeIn)
        {
            conversationAudioSource.Stop();
        }

        fadeCoroutine = null;
    }

    private System.Collections.IEnumerator FadeRoomAudio(bool duck)
    {
        if (roomAudioZones == null || roomAudioZones.Length == 0) yield break;

        float elapsed = 0f;

        // Store current volumes as starting point
        float[] startVolumes = new float[roomAudioZones.Length];
        for (int i = 0; i < roomAudioZones.Length; i++)
        {
            if (roomAudioZones[i] != null && roomAudioZones[i].roomMusic != null)
                startVolumes[i] = roomAudioZones[i].roomMusic.volume;
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
                roomAudioZones[i].roomMusic.volume = duck ? roomAudioDuckVolume : originalRoomVolumes[i];
            }
        }

        Debug.Log($"[InteractiveItem] {name}: Room audio {(duck ? "ducked to " + roomAudioDuckVolume : "restored")}");
        roomAudioFadeCoroutine = null;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
#endif
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
