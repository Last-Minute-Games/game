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
        
        [System.Serializable]
        public class DayWakeUpDialogue
        {
            [Tooltip("Day flag to check for (e.g., 'day.two', 'day.three', 'day.four')")]
            public string dayFlag;
            
            [Tooltip("Dialogue graph to play when waking up on this day (after returning from battle)")]
            public DialogNodeGraph dialogueGraph;
        }
        
        [Header("Day-Specific Wake-Up Dialogues")]
        [Tooltip("Array of day-specific dialogues. Each entry specifies which day flag to check and which dialogue to play when waking up.")]
        [SerializeField] private DayWakeUpDialogue[] dayWakeUpDialogues;
        
        [Header("HUD")]
        [SerializeField] private bool autoFindHudInitializer = true;
        
        [Header("Battle Result Time Adjustments")]
        [Tooltip("Seconds to add to clock when player wins a battle")]
        [SerializeField] private float winTimeBonus = 30f;
        
        [Tooltip("Seconds to subtract from clock when player loses a battle")]
        [SerializeField] private float loseTimePenalty = 30f;
        
        [Header("Day Six Ending")]
        [Tooltip("EndTransition component to trigger when day.six is detected")]
        [SerializeField] private EndTransition endTransition;
        
        [Tooltip("Auto-find EndTransition if not assigned")]
        [SerializeField] private bool autoFindEndTransition = true;
        
        private bool hasPlayed = false;
        
        // Add a flag to determine which sequence to play
        private bool shouldPlayFullWakeUpCutscene = false;
        
        private IEnumerator BeginWakeUpSequence()
        {
            Debug.Log("[OverworldWakeUpCutscene] BeginWakeUpSequence started");
            
            if (hasPlayed) yield break;
            hasPlayed = true;
            
            
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
            
            
            // Start ClockTimer NOW (after cutscene completes)
            if (clockTimer != null)
            {
                clockTimer.StartTimer(clockTimer.totalTime);
                Debug.Log("[OverworldWakeUpCutscene] Clock timer started after cutscene");
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
        
        private void Awake()
        {
            // No longer need to set flags in Awake - we'll handle everything in PlayDaySpecificWakeUpDialogue
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

            // Find ClockTimer
            if (clockTimer == null)
            {
                clockTimer = FindObjectOfType<ClockTimer>();
            }
            
            // Find EndTransition if needed
            if (autoFindEndTransition && endTransition == null)
            {
                endTransition = FindObjectOfType<EndTransition>();
                if (endTransition != null)
                {
                    Debug.Log("[OverworldWakeUpCutscene] Found EndTransition component");
                }
            }

            // Check if we should play the cutscene (via PlayerPrefs flag from TutorialScene)
            int playFlag = PlayerPrefs.GetInt("PlayWakeUpCutscene", 0);
            Debug.Log($"[OverworldWakeUpCutscene] PlayWakeUpCutscene flag value: {playFlag}");

            if (playFlag == 1)
            {
                // Clear the flag
                PlayerPrefs.SetInt("PlayWakeUpCutscene", 0);
                PlayerPrefs.Save();
                
                Debug.Log("[OverworldWakeUpCutscene] Setting up full wake-up cutscene...");
                
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
            // Check if day.six flag is set - trigger ending instead of dialogue
            else if (GameFlags.HasFlag("day.six"))
            {
                Debug.Log("[OverworldWakeUpCutscene] day.six detected - triggering ending sequence");
                StartCoroutine(HandleDayEndingSequence("day.six"));
            }
            // NOTE: day.five does NOT trigger ending here - it triggers at END of day via ClockTimer
            // Check if we should play day-specific wake-up dialogue
            else if (ShouldPlayDaySpecificWakeUpDialogue())
            {
                string currentDay = GetCurrentDay();
                Debug.Log($"[OverworldWakeUpCutscene] Day-specific wake-up dialogue detected for {currentDay}, starting...");
                StartCoroutine(PlayDaySpecificWakeUpDialogue());
            }
            else
            {
                Debug.Log("[OverworldWakeUpCutscene] No cutscene to play, component will remain inactive");
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
        
        /// <summary>
        /// Call this method to trigger the full wake-up cutscene (day one initial wake up)
        /// </summary>
        public void TriggerFullWakeUpCutscene()
        {
            Debug.Log("[OverworldWakeUpCutscene] TriggerFullWakeUpCutscene() called");
            shouldPlayFullWakeUpCutscene = true;
        }
        
        /// <summary>
        /// Static method to trigger the wake-up cutscene from other scenes (via PlayerPrefs flag)
        /// </summary>
        public static void TriggerWakeUpCutscene()
        {
            Debug.Log("[OverworldWakeUpCutscene] TriggerWakeUpCutscene() static method called - setting PlayerPrefs flag");
            PlayerPrefs.SetInt("PlayWakeUpCutscene", 1);
            PlayerPrefs.Save();
            Debug.Log($"[OverworldWakeUpCutscene] Flag set to: {PlayerPrefs.GetInt("PlayWakeUpCutscene")}");
        }
        
        /// <summary>
        /// Check if we should play day-specific wake-up dialogue
        /// </summary>
        private bool ShouldPlayDaySpecificWakeUpDialogue()
        {
            if (dayWakeUpDialogues == null || dayWakeUpDialogues.Length == 0)
            {
                return false;
            }

            // Check each day in the array
            foreach (var dayDialogue in dayWakeUpDialogues)
            {
                if (dayDialogue == null || string.IsNullOrEmpty(dayDialogue.dayFlag) || dayDialogue.dialogueGraph == null)
                {
                    continue;
                }

                // Check if this day flag is set and the next day flag is not set
                if (GameFlags.HasFlag(dayDialogue.dayFlag))
                {
                    // Determine what the "next" day flag would be
                    string nextDayFlag = GetNextDayFlag(dayDialogue.dayFlag);
                    
                    // If there's a next day flag, only trigger if it's not set
                    // If there's no next day flag, just check if current day is set
                    if (string.IsNullOrEmpty(nextDayFlag) || !GameFlags.HasFlag(nextDayFlag))
                    {
                        Debug.Log($"[OverworldWakeUpCutscene] {dayDialogue.dayFlag} wake-up dialogue available");
                        return true;
                    }
                }
            }

            return false;
        }
        
        /// <summary>
        /// Get the dialogue graph for the current day
        /// </summary>
        private DialogNodeGraph GetDaySpecificWakeUpDialogue()
        {
            if (dayWakeUpDialogues == null || dayWakeUpDialogues.Length == 0)
            {
                return null;
            }

            // Find the matching day dialogue
            foreach (var dayDialogue in dayWakeUpDialogues)
            {
                if (dayDialogue == null || string.IsNullOrEmpty(dayDialogue.dayFlag) || dayDialogue.dialogueGraph == null)
                {
                    continue;
                }

                if (GameFlags.HasFlag(dayDialogue.dayFlag))
                {
                    // Determine what the "next" day flag would be
                    string nextDayFlag = GetNextDayFlag(dayDialogue.dayFlag);
                    
                    // If there's a next day flag, only return if it's not set
                    // If there's no next day flag, just return the current day
                    if (string.IsNullOrEmpty(nextDayFlag) || !GameFlags.HasFlag(nextDayFlag))
                    {
                        return dayDialogue.dialogueGraph;
                    }
                }
            }

            return null;
        }
        
        /// <summary>
        /// Get the next day flag for a given day flag (used to check if we're still on that day)
        /// </summary>
        private string GetNextDayFlag(string currentDayFlag)
        {
            switch (currentDayFlag)
            {
                case "day.one": return "day.two";
                case "day.two": return "day.three";
                case "day.three": return "day.four";
                case "day.four": return "day.five";
                case "day.five": return "day.six"; // day.five progresses to day.six after timer runs out (ClockTimer handles this)
                case "day.six": return null; // day.six triggers ending immediately
                default: return null;
            }
        }
        
        /// <summary>
        /// Get the current day flag that is active (for use in determining timer start behavior)
        /// </summary>
        private string GetCurrentDay()
        {
            if (dayWakeUpDialogues == null || dayWakeUpDialogues.Length == 0)
            {
                return null;
            }

            // Find the matching day dialogue
            foreach (var dayDialogue in dayWakeUpDialogues)
            {
                if (dayDialogue == null || string.IsNullOrEmpty(dayDialogue.dayFlag) || dayDialogue.dialogueGraph == null)
                {
                    continue;
                }

                if (GameFlags.HasFlag(dayDialogue.dayFlag))
                {
                    // Determine what the "next" day flag would be
                    string nextDayFlag = GetNextDayFlag(dayDialogue.dayFlag);
                    
                    // If there's a next day flag, only return if it's not set
                    // If there's no next day flag, just return the current day
                    if (string.IsNullOrEmpty(nextDayFlag) || !GameFlags.HasFlag(nextDayFlag))
                    {
                        return dayDialogue.dayFlag;
                    }
                }
            }

            return null;
        }
        
        /// <summary>
        /// Play day-specific wake-up dialogue after clock reconstruction completes
        /// </summary>
        private IEnumerator PlayDaySpecificWakeUpDialogue()
        {
            Debug.Log("[OverworldWakeUpCutscene] Starting day-specific wake-up dialogue sequence");
            
            // Find clock timer
            if (clockTimer == null)
            {
                clockTimer = FindObjectOfType<ClockTimer>();
            }
            
            if (clockTimer == null)
            {
                Debug.LogWarning("[OverworldWakeUpCutscene] ClockTimer not found - cannot continue");
                yield break;
            }

            // Get the current day to determine behavior
            string currentDay = GetCurrentDay();
            
            if (currentDay == "day.one")
            {
                // Day.one: Just start timer normally (no reconstruction needed)
                Debug.Log("[OverworldWakeUpCutscene] Day.one - starting timer normally without reconstruction");
                clockTimer.StartTimer(clockTimer.totalTime);
                yield break;
            }
            
            // Days 2-5: Call clock reconstruction directly (without starting timer)
            Debug.Log($"[OverworldWakeUpCutscene] {currentDay} detected - calling clock reconstruction");
            
            // Call the clock reconstruction coroutine directly and wait for it to complete
            yield return StartCoroutine(clockTimer.ReconstructClock());
            
            Debug.Log("[OverworldWakeUpCutscene] Clock reconstruction complete");
            
            // Apply battle result time adjustment after reconstruction
            CheckAndApplyBattleResultTimeAdjustment();
            
            // Check for evidence flags and unlock corresponding cards (every day)
            CheckAndSetEvidenceCardFlags();

            // Get the dialogue for current day
            DialogNodeGraph dialogueGraph = GetDaySpecificWakeUpDialogue();
            
            // Ensure dialogBehaviour is found if not already assigned
            if (dialogBehaviour == null)
            {
                dialogBehaviour = FindFirstObjectByType<DialogBehaviour>();
                if (dialogBehaviour != null)
                {
                    Debug.Log("[OverworldWakeUpCutscene] Found DialogBehaviour via FindFirstObjectByType");
                }
            }
            
            if (dialogueGraph == null)
            {
                Debug.LogWarning("[OverworldWakeUpCutscene] Cannot play day-specific dialogue - dialogueGraph is null");
                
                // Start timer manually if dialogue won't play
                if (clockTimer != null)
                {
                    clockTimer.StartTimer(clockTimer.totalTime);
                    Debug.Log("[OverworldWakeUpCutscene] Started clock timer manually (no dialogue to play)");
                }
                yield break;
            }
            
            if (dialogBehaviour == null)
            {
                Debug.LogWarning("[OverworldWakeUpCutscene] Cannot play day-specific dialogue - dialogBehaviour is null");
                
                // Start timer manually if dialogue won't play
                if (clockTimer != null)
                {
                    clockTimer.StartTimer(clockTimer.totalTime);
                    Debug.Log("[OverworldWakeUpCutscene] Started clock timer manually (no dialogBehaviour)");
                }
                yield break;
            }
            
            Debug.Log($"[OverworldWakeUpCutscene] Dialogue graph found: {dialogueGraph.name}, DialogBehaviour found: {dialogBehaviour.name}");

            // Set player dialogue state
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            CharacterMotor2D playerMotor = null;
            if (playerObj != null)
            {
                playerMotor = playerObj.GetComponent<CharacterMotor2D>();
                if (playerMotor != null)
                {
                    playerMotor.SetDialogueActive(true);
                    Debug.Log("[OverworldWakeUpCutscene] Player dialogue state set to active");
                }
            }

            // Wait for dialogue to finish
            bool dialogueFinished = false;
            UnityEngine.Events.UnityAction onFinished = () => { dialogueFinished = true; Debug.Log("[OverworldWakeUpCutscene] Dialogue finished callback triggered"); };
            dialogBehaviour.OnDialogFinished.AddListener(onFinished);
            
            // Also listen for when dialogue starts
            bool dialogueStarted = false;
            UnityEngine.Events.UnityAction onStarted = () => { dialogueStarted = true; Debug.Log("[OverworldWakeUpCutscene] Dialogue started callback triggered"); };
            dialogBehaviour.OnDialogStarted.AddListener(onStarted);
            
            Debug.Log($"[OverworldWakeUpCutscene] Starting day-specific wake-up dialogue: {dialogueGraph.name} (DialogBehaviour: {dialogBehaviour.name})");
            Debug.Log($"[OverworldWakeUpCutscene] DialogBehaviour enabled: {dialogBehaviour.enabled}, gameObject active: {dialogBehaviour.gameObject.activeInHierarchy}");
            
            try
            {
                dialogBehaviour.StartDialog(dialogueGraph);
                Debug.Log("[OverworldWakeUpCutscene] StartDialog() called successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[OverworldWakeUpCutscene] Error starting dialogue: {ex.Message}\n{ex.StackTrace}");
            }
            
            // Wait a moment to see if dialogue starts
            yield return new WaitForSeconds(0.5f);
            if (!dialogueStarted)
            {
                Debug.LogWarning("[OverworldWakeUpCutscene] Dialogue did not start within 0.5 seconds - may be an issue with dialogue system");
            }
            
            while (!dialogueFinished)
            {
                yield return null;
            }
            
            dialogBehaviour.OnDialogFinished.RemoveListener(onFinished);
            dialogBehaviour.OnDialogStarted.RemoveListener(onStarted);

            // Reset player dialogue state
            if (playerMotor != null)
            {
                playerMotor.SetDialogueActive(false);
            }

            // NOW start the clock timer after dialogue completes
            if (clockTimer != null)
            {
                clockTimer.StartTimer(clockTimer.totalTime);
                Debug.Log("[OverworldWakeUpCutscene] Clock timer started after day-specific dialogue completed");
            }
            else
            {
                Debug.LogWarning("[OverworldWakeUpCutscene] ClockTimer not found - cannot start timer!");
            }

            Debug.Log("[OverworldWakeUpCutscene] Day-specific wake-up dialogue complete");
        }
        
        /// <summary>
        /// Handle the special day ending sequence (day.five or day.six)
        /// </summary>
        private IEnumerator HandleDayEndingSequence(string dayFlag)
        {
            Debug.Log($"[OverworldWakeUpCutscene] Starting ending sequence for {dayFlag}");
            
            // Pause the environment using GlobalPause
            GlobalPause.SetMinigamePaused(true);
            Debug.Log("[OverworldWakeUpCutscene] Environment paused for ending sequence");
            
            // Optional: Add a brief delay before triggering ending
            yield return new WaitForSeconds(1f);
            
            // Set the start.ending flag to trigger the ending
            if (!GameFlags.HasFlag("start.ending"))
            {
                Debug.Log($"[OverworldWakeUpCutscene] Setting start.ending flag for {dayFlag}");
                GameFlags.SetFlag("start.ending");
            }
            
            // Trigger the ending transition
            if (endTransition != null)
            {
                Debug.Log($"[OverworldWakeUpCutscene] Triggering EndTransition for {dayFlag}");
                endTransition.TriggerEndTransition();
            }
            else
            {
                Debug.LogError("[OverworldWakeUpCutscene] EndTransition component not found! Cannot trigger ending.");
                
                // Fallback: unpause if we can't trigger ending
                GlobalPause.SetMinigamePaused(false);
            }
            
            Debug.Log($"[OverworldWakeUpCutscene] Ending sequence initiated for {dayFlag}");
        }
        
        /// <summary>
        /// Check for evidence flags and set corresponding card flags for the ending.
        /// This runs every day (days 2-5) so cards are unlocked as evidence is collected.
        /// </summary>
        private void CheckAndSetEvidenceCardFlags()
        {
            Debug.Log("[OverworldWakeUpCutscene] Checking evidence flags for card unlocks...");
            
            // evidence.knife -> card.shield_slash
            if (GameFlags.HasFlag("evidence.knife"))
            {
                if (!GameFlags.HasFlag("card.shield_slash"))
                {
                    GameFlags.SetFlag("card.shield_slash");
                    Debug.Log("[OverworldWakeUpCutscene] Evidence: knife found -> Unlocked card: shield_slash");
                }
                else
                {
                    Debug.Log("[OverworldWakeUpCutscene] Evidence: knife found (card.shield_slash already unlocked)");
                }
            }
            
            // evidence.throne -> card.dramatic_exit
            if (GameFlags.HasFlag("evidence.throne"))
            {
                if (!GameFlags.HasFlag("card.dramatic_exit"))
                {
                    GameFlags.SetFlag("card.dramatic_exit");
                    Debug.Log("[OverworldWakeUpCutscene] Evidence: throne found -> Unlocked card: dramatic_exit");
                }
                else
                {
                    Debug.Log("[OverworldWakeUpCutscene] Evidence: throne found (card.dramatic_exit already unlocked)");
                }
            }
            
            // evidence.silverware -> card.tariff_strike
            if (GameFlags.HasFlag("evidence.silverware"))
            {
                if (!GameFlags.HasFlag("card.tariff_strike"))
                {
                    GameFlags.SetFlag("card.tariff_strike");
                    Debug.Log("[OverworldWakeUpCutscene] Evidence: silverware found -> Unlocked card: tariff_strike");
                }
                else
                {
                    Debug.Log("[OverworldWakeUpCutscene] Evidence: silverware found (card.tariff_strike already unlocked)");
                }
            }
            
            Debug.Log("[OverworldWakeUpCutscene] Evidence flag check complete");
        }
        
        /// <summary>
        /// Check for nether.win or nether.lose flags and adjust ClockTimer accordingly.
        /// Clears the flags after applying the time adjustment and saves the game.
        /// </summary>
        private void CheckAndApplyBattleResultTimeAdjustment()
        {
            // Find ClockTimer if not already found
            if (clockTimer == null)
            {
                clockTimer = FindObjectOfType<ClockTimer>();
            }
            
            if (clockTimer == null)
            {
                Debug.LogWarning("[OverworldWakeUpCutscene] ClockTimer not found - cannot apply battle result time adjustment");
                return;
            }
            
            bool timeWasAdjusted = false;
            
            // Check for win flag
            if (GameFlags.HasFlag("nether.win"))
            {
                Debug.Log($"[OverworldWakeUpCutscene] ? Battle won! Adding {winTimeBonus} seconds to clock timer");
                clockTimer.AddTime(winTimeBonus);
                GameFlags.RemoveFlag("nether.win");
                Debug.Log("[OverworldWakeUpCutscene] Cleared nether.win flag");
                timeWasAdjusted = true;
            }
            // Check for lose flag
            else if (GameFlags.HasFlag("nether.lose"))
            {
                Debug.Log($"[OverworldWakeUpCutscene] ? Battle lost! Subtracting {loseTimePenalty} seconds from clock timer");
                clockTimer.RemoveTime(loseTimePenalty);
                GameFlags.RemoveFlag("nether.lose");
                Debug.Log("[OverworldWakeUpCutscene] Cleared nether.lose flag");
                timeWasAdjusted = true;
            }
            else
            {
                Debug.Log("[OverworldWakeUpCutscene] No battle result flags detected (nether.win or nether.lose)");
            }
            
            // Save the game if time was adjusted to persist the new clock time
            if (timeWasAdjusted)
            {
                Debug.Log("[OverworldWakeUpCutscene] Saving game after clock time adjustment...");
                GameFlagsManager.SaveCurrentGame();
                Debug.Log("[OverworldWakeUpCutscene] Game saved successfully with new clock time");
            }
        }
    }
}