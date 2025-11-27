using System.Collections;
using cherrydev;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

namespace Systems.Overworld.Intro
{
    public class OverworldWakeUpCutscene : MonoBehaviour
    {
        private CanvasGroup fadeCanvasGroup;
        private SpriteRenderer sleepingMainSpriteRenderer;
        private SpriteRenderer mainCharSpriteRenderer;
        private UnityEngine.Rendering.Universal.ShadowCaster2D mainCharShadowCaster;
        private PlayerInput2D playerInput;
        private Camera mainCamera;
        private CinemachineBrain cinemachineBrain;
        private ClockTimer clockTimer;
        
        [Header("GameObjects")]
        [SerializeField] private GameObject sleepingMain;
        
        [Header("Sprites")]
        [SerializeField] private Sprite nikolausSleepSprite;
        [SerializeField] private Sprite nikolausAwakeSprite;
        
        [Header("Dialogue")]
        [SerializeField] private DialogBehaviour dialogBehaviour;
        [SerializeField] private DialogNodeGraph wakeUpDialogGraph;
        
        [Header("HUD")]
        [SerializeField] private HudInitializer hudInitializer;
        [SerializeField] private bool autoFindHudInitializer = true;
        
        private bool hasPlayed = false;
        
        private IEnumerator BeginWakeUpSequence()
        {
            Debug.Log("[OverworldWakeUpCutscene] BeginWakeUpSequence started");
            
            if (hasPlayed) yield break;
            hasPlayed = true;
            
            // Find HUD initializer if needed
            if (hudInitializer == null && autoFindHudInitializer)
            {
                hudInitializer = FindObjectOfType<HudInitializer>();
                if (hudInitializer != null)
                {
                    Debug.Log("[OverworldWakeUpCutscene] Found HudInitializer");
                }
            }
            
            // Find and pause ClockTimer
            if (clockTimer == null)
            {
                clockTimer = FindObjectOfType<ClockTimer>();
            }
            
            if (clockTimer != null)
            {
                clockTimer.PauseTimer(true);
                Debug.Log("[OverworldWakeUpCutscene] Clock timer paused");
            }
            else
            {
                Debug.LogWarning("[OverworldWakeUpCutscene] ClockTimer not found in scene");
            }
            
            // Enable SleepingMain sprite renderer
            if (sleepingMainSpriteRenderer != null)
            {
                sleepingMainSpriteRenderer.enabled = true;
                Debug.Log("[OverworldWakeUpCutscene] SleepingMain sprite renderer enabled");
            }
            
            // Disable MainCharacter sprite renderer
            if (mainCharSpriteRenderer != null)
            {
                mainCharSpriteRenderer.enabled = false;
                Debug.Log("[OverworldWakeUpCutscene] MainCharacter sprite renderer disabled");
            }
            
            // Disable MainCharacter shadow caster
            if (mainCharShadowCaster != null)
            {
                mainCharShadowCaster.enabled = false;
                Debug.Log("[OverworldWakeUpCutscene] MainCharacter ShadowCaster2D disabled");
            }
            
            // Disable CinemachineBrain and set camera position
            if (cinemachineBrain != null)
            {
                cinemachineBrain.enabled = false;
                Debug.Log("[OverworldWakeUpCutscene] CinemachineBrain disabled");
            }
            
            if (mainCamera != null)
            {
                Vector3 sleepingPosition = sleepingMain.transform.position;
                mainCamera.transform.position = new Vector3(sleepingPosition.x, sleepingPosition.y, -10f);
                Debug.Log($"[OverworldWakeUpCutscene] Main camera position set to {mainCamera.transform.position}");
            }
            
            // Disable player input during cutscene
            if (playerInput != null)
            {
                playerInput.isInputEnabled = false;
                Debug.Log("[OverworldWakeUpCutscene] Player input disabled");
            }
            
            yield return new WaitForSeconds(1.5f);
            
            // Use ScreenFader's eyes opening effect instead of regular fade
            Debug.Log("[OverworldWakeUpCutscene] Opening eyes (using ScreenFader)");
            ScreenFader screenFader = FindFirstObjectByType<ScreenFader>();
            if (screenFader != null)    
            {
                yield return StartCoroutine(screenFader.EyesOpeningEffect());
            }
            else
            {
                // Fallback to DOTween fade if ScreenFader is not found
                Debug.LogWarning("[OverworldWakeUpCutscene] ScreenFader not found, using fallback fade");
                fadeCanvasGroup.DOFade(0f, 3f).SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    fadeCanvasGroup.blocksRaycasts = false;
                    Debug.Log("[OverworldWakeUpCutscene] Fade complete");
                });
                yield return new WaitForSeconds(3.5f);
            }
            
            // Change sprite to awake
            Debug.Log("[OverworldWakeUpCutscene] Changing sprite to awake");
            if (sleepingMainSpriteRenderer != null && nikolausAwakeSprite != null)
            {
                sleepingMainSpriteRenderer.sprite = nikolausAwakeSprite;
            }
            
            yield return new WaitForSeconds(1.5f);
            
            // Start dialogue when eyes open
            Debug.Log("[OverworldWakeUpCutscene] Starting dialogue after waking");
            
            bool dialogueFinished = false;
            
            if (dialogBehaviour != null && wakeUpDialogGraph != null)
            {
                // Add listener for when dialogue finishes
                UnityEngine.Events.UnityAction onFinished = () => { dialogueFinished = true; };
                dialogBehaviour.OnDialogFinished.AddListener(onFinished);
                
                dialogBehaviour.StartDialog(wakeUpDialogGraph);
                
                // Wait for dialogue to finish
                while (!dialogueFinished)
                {
                    yield return null;
                }
                
                // Remove the listener
                dialogBehaviour.OnDialogFinished.RemoveListener(onFinished);
            }
            else
            {
                Debug.Log("Nikolaus: Was that... just a dream?");
                yield return new WaitForSeconds(2f); // Fallback wait time
            }
            
            Debug.Log("[OverworldWakeUpCutscene] Dialogue finished, starting transition");
            
            // Fade to black
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.DOFade(1f, 2f).SetEase(Ease.InOutQuad);
            
            yield return new WaitForSeconds(2.5f);
            
            // Hide SleepingMain sprite renderer while screen is black
            if (sleepingMainSpriteRenderer != null)
            {
                sleepingMainSpriteRenderer.enabled = false;
                Debug.Log("[OverworldWakeUpCutscene] SleepingMain sprite renderer disabled");
            }
            
            // Re-enable MainCharacter sprite renderer
            if (mainCharSpriteRenderer != null)
            {
                mainCharSpriteRenderer.enabled = true;
                Debug.Log("[OverworldWakeUpCutscene] MainCharacter sprite renderer enabled");
            }
            
            // Re-enable MainCharacter shadow caster
            if (mainCharShadowCaster != null)
            {
                mainCharShadowCaster.enabled = true;
                Debug.Log("[OverworldWakeUpCutscene] MainCharacter ShadowCaster2D enabled");
            }
            
            // Re-enable CinemachineBrain
            if (cinemachineBrain != null)
            {
                cinemachineBrain.enabled = true;
                Debug.Log("[OverworldWakeUpCutscene] CinemachineBrain enabled");
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // Fade from black - character is now out of bed
            fadeCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                fadeCanvasGroup.blocksRaycasts = false;
                Debug.Log("[OverworldWakeUpCutscene] Fade complete - character out of bed");
                
                // Re-enable player input immediately after fade completes
                if (playerInput != null)
                {
                    playerInput.isInputEnabled = true;
                    Debug.Log("[OverworldWakeUpCutscene] Player input enabled");
                }
            });
            
            yield return new WaitForSeconds(0.5f); // Wait for fade to complete
            
            // TRIGGER HUD INITIALIZATION ANIMATION
            // This happens after the player is out of bed and can see the game world
            if (hudInitializer != null)
            {
                Debug.Log("[OverworldWakeUpCutscene] Triggering HUD initialization");
                hudInitializer.TriggerAnimation();
            }
            else
            {
                Debug.LogWarning("[OverworldWakeUpCutscene] HudInitializer not found, cannot trigger HUD animation");
            }
            
            // Resume ClockTimer at the same time as HUD animation
            if (clockTimer != null)
            {
                clockTimer.PauseTimer(false);
                Debug.Log("[OverworldWakeUpCutscene] Clock timer resumed");
            }
            
            yield return new WaitForSeconds(2f); // Wait for HUD animation to play
            
            // SET DAY.ONE FLAG - This is the start of the game's day progression
            // This will automatically trigger a save because day flags auto-save
            if (!GameFlags.HasFlag("day.one"))
            {
                Debug.Log("[OverworldWakeUpCutscene] Setting day.one flag (game start)");
                GameFlags.SetFlag("day.one");
            }
            
            Debug.Log("[OverworldWakeUpCutscene] Complete");
            yield return null;
        }
        
        void Start()
        {
            Debug.Log("[OverworldWakeUpCutscene] Start() called");

            // Find main character components first
            var mainChar = GameObject.Find("MainCharacter");
            if (mainChar != null)
            {
                playerInput = mainChar.GetComponent<PlayerInput2D>();
                mainCharSpriteRenderer = mainChar.GetComponent<SpriteRenderer>();
                mainCharShadowCaster = mainChar.GetComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>();
                Debug.Log("[OverworldWakeUpCutscene] Found MainCharacter for input control, sprite renderer, and shadow caster");
            }
            else
            {
                Debug.LogError("[OverworldWakeUpCutscene] MainCharacter not found!");
            }
            
            // Find MainCamera and CinemachineBrain
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
                Debug.Log("[OverworldWakeUpCutscene] Found MainCamera and CinemachineBrain");
            }
            else
            {
                Debug.LogError("[OverworldWakeUpCutscene] MainCamera not found!");
            }
            
            // Set up SleepingMain sprite renderer
            if (sleepingMain != null)
            {
                sleepingMainSpriteRenderer = sleepingMain.GetComponent<SpriteRenderer>();
                sleepingMainSpriteRenderer.enabled = false;
                
                // Set to sleeping sprite (will be enabled when cutscene starts)
                if (sleepingMainSpriteRenderer != null && nikolausSleepSprite != null)
                {
                    sleepingMainSpriteRenderer.sprite = nikolausSleepSprite;
                    Debug.Log("[OverworldWakeUpCutscene] Set SleepingMain sprite to sleeping");
                }
            }
            else
            {
                Debug.LogError("[OverworldWakeUpCutscene] SleepingMain GameObject not assigned!");
            }
            
            // Find fade canvas
            fadeCanvasGroup = GameObject.Find("FadeCanvasGroup")?.GetComponent<CanvasGroup>();
            if (fadeCanvasGroup != null)
            {
                Debug.Log("[OverworldWakeUpCutscene] Found FadeCanvasGroup");
            }
            else
            {
                Debug.LogError("[OverworldWakeUpCutscene] FadeCanvasGroup not found!");
            }
            
            // Find dialogue system if not assigned
            if (dialogBehaviour == null)
            {
                dialogBehaviour = FindFirstObjectByType<DialogBehaviour>();
            }

            // Check if we should play the cutscene
            int playFlag = PlayerPrefs.GetInt("PlayWakeUpCutscene", 0);
            Debug.Log($"[OverworldWakeUpCutscene] Flag value: {playFlag}");

            if (playFlag != 1)
            {
                Debug.Log("[OverworldWakeUpCutscene] Flag not set, disabling");
                enabled = false;
                return;
            }

            // Clear the flag
            PlayerPrefs.SetInt("PlayWakeUpCutscene", 0);
            PlayerPrefs.Save();
            
            Debug.Log("[OverworldWakeUpCutscene] Setting up cutscene...");
            
            // Set up ScreenFader with eyes ALREADY closed at start (player is waking up)
            ScreenFader screenFader = FindFirstObjectByType<ScreenFader>();
            if (screenFader != null)
            {
                Debug.Log("[OverworldWakeUpCutscene] Setting up eyes closed position (player waking up)");
                StartCoroutine(SetupEyesAlreadyClosedState(screenFader));
            }
            else
            {
                // Fallback: use fade canvas if ScreenFader not available
                if (fadeCanvasGroup != null)
                {
                    fadeCanvasGroup.alpha = 1f; // Start opaque (black screen)
                    Debug.Log("[OverworldWakeUpCutscene] Fade canvas set to black (fallback)");
                }
                
                Debug.Log("[OverworldWakeUpCutscene] Starting cutscene coroutine");
                StartCoroutine(BeginWakeUpSequence());
            }
        }
        
        private IEnumerator SetupEyesAlreadyClosedState(ScreenFader screenFader)
        {
            // Create the panels manually and position them in closed state (covering screen)
            // WITHOUT animating them - they should already be closed
            
            // We need to trigger the eyes closing effect to create the panels,
            // but we'll do it instantly (duration 0) so they appear closed immediately
            float originalDuration = screenFader.splitPanelDuration;
            screenFader.splitPanelDuration = 0f; // Instant
            
            yield return StartCoroutine(screenFader.EyesClosingEffect());
            
            // Restore original duration for the opening effect
            screenFader.splitPanelDuration = originalDuration;
            
            // Make sure fade canvas is hidden
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
            }
            
            Debug.Log("[OverworldWakeUpCutscene] Eyes already closed state set up, starting cutscene");
            yield return null; // Wait one frame for everything to be set up
            StartCoroutine(BeginWakeUpSequence());
        }
        
        // Static method to trigger the cutscene from other scenes
        public static void TriggerWakeUpCutscene()
        {
            Debug.Log("[OverworldWakeUpCutscene] TriggerWakeUpCutscene() called");
            
            // Clear the journal tutorial flag so it shows again in Overworld
            // The TutorialScene showed the journal as part of the tutorial, but in Overworld
            // the player needs to learn to open it themselves
            if (GameFlags.HasFlag("journal.tutorial.shown"))
            {
                GameFlags.RemoveFlag("journal.tutorial.shown");
                Debug.Log("[OverworldWakeUpCutscene] Cleared 'journal.tutorial.shown' flag for Overworld");
            }
            
            PlayerPrefs.SetInt("PlayWakeUpCutscene", 1);
            PlayerPrefs.Save();
            Debug.Log($"[OverworldWakeUpCutscene] Flag set to: {PlayerPrefs.GetInt("PlayWakeUpCutscene")}" +
                      $" [next scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1}]");
        }
    }
}