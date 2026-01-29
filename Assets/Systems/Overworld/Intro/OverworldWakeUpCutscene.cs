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
            DebugLogger.LogCutscene("BeginWakeUpSequence started");
            
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
                DebugLogger.LogCutscene("Clock timer paused");
            }
            else
            {
                DebugLogger.LogWarning("[OverworldWakeUpCutscene] ClockTimer not found in scene");
            }
            
            // Enable SleepingMain sprite renderer
            if (sleepingMainSpriteRenderer != null)
            {
                sleepingMainSpriteRenderer.enabled = true;
                DebugLogger.LogCutscene("SleepingMain sprite renderer enabled");
            }
            
            // Disable MainCharacter sprite renderer
            if (mainCharSpriteRenderer != null)
            {
                mainCharSpriteRenderer.enabled = false;
                DebugLogger.LogCutscene("MainCharacter sprite renderer disabled");
            }
            
            // Disable MainCharacter shadow caster
            if (mainCharShadowCaster != null)
            {
                mainCharShadowCaster.enabled = false;
                DebugLogger.LogCutscene("MainCharacter ShadowCaster2D disabled");
            }
            
            // Disable CinemachineBrain and set camera position
            if (cinemachineBrain != null)
            {
                cinemachineBrain.enabled = false;
                DebugLogger.LogCutscene("CinemachineBrain disabled");
            }
            
            if (mainCamera != null)
            {
                Vector3 sleepingPosition = sleepingMain.transform.position;
                mainCamera.transform.position = new Vector3(sleepingPosition.x, sleepingPosition.y, -10f);
                DebugLogger.LogCutscene($"Main camera position set to {mainCamera.transform.position}");
            }
            
            // Disable player input during cutscene
            if (playerInput != null)
            {
                playerInput.isInputEnabled = false;
                DebugLogger.LogCutscene("Player input disabled");
            }
            
            yield return new WaitForSeconds(1.5f);
            
            // Use ScreenFader's eyes opening effect instead of regular fade
            DebugLogger.LogCutscene("Opening eyes (using ScreenFader)");
            ScreenFader screenFader = FindFirstObjectByType<ScreenFader>();
            if (screenFader != null)    
            {
                yield return StartCoroutine(screenFader.EyesOpeningEffect());
            }
            else
            {
                // Fallback to DOTween fade if ScreenFader is not found
                DebugLogger.LogWarning("[OverworldWakeUpCutscene] ScreenFader not found, using fallback fade");
                fadeCanvasGroup.DOFade(0f, 3f).SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    fadeCanvasGroup.blocksRaycasts = false;
                    DebugLogger.LogCutscene("Fade complete");
                });
                yield return new WaitForSeconds(3.5f);
            }
            
            // Change sprite to awake
            DebugLogger.LogCutscene("Changing sprite to awake");
            if (sleepingMainSpriteRenderer != null && nikolausAwakeSprite != null)
            {
                sleepingMainSpriteRenderer.sprite = nikolausAwakeSprite;
            }
            
            yield return new WaitForSeconds(1.5f);
            
            // Start dialogue when eyes open
            DebugLogger.LogCutscene("Starting dialogue after waking");
            
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
                DebugLogger.LogCutscene("Nikolaus: Was that... just a dream?");
                yield return new WaitForSeconds(2f); // Fallback wait time
            }
            
            DebugLogger.LogCutscene("Dialogue finished, starting transition");
            
            // Fade to black
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.DOFade(1f, 2f).SetEase(Ease.InOutQuad);
            
            yield return new WaitForSeconds(2.5f);
            
            // Hide SleepingMain sprite renderer while screen is black
            if (sleepingMainSpriteRenderer != null)
            {
                sleepingMainSpriteRenderer.enabled = false;
                DebugLogger.LogCutscene("SleepingMain sprite renderer disabled");
            }
            
            // Re-enable MainCharacter sprite renderer
            if (mainCharSpriteRenderer != null)
            {
                mainCharSpriteRenderer.enabled = true;
                DebugLogger.LogCutscene("MainCharacter sprite renderer enabled");
            }
            
            // Re-enable MainCharacter shadow caster
            if (mainCharShadowCaster != null)
            {
                mainCharShadowCaster.enabled = true;
                DebugLogger.LogCutscene("MainCharacter ShadowCaster2D enabled");
            }
            
            // Re-enable CinemachineBrain
            if (cinemachineBrain != null)
            {
                cinemachineBrain.enabled = true;
                DebugLogger.LogCutscene("CinemachineBrain enabled");
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // Fade from black - character is now out of bed
            fadeCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                fadeCanvasGroup.blocksRaycasts = false;
                DebugLogger.LogCutscene("Fade complete - character out of bed");
                
                // Re-enable player input immediately after fade completes
                if (playerInput != null)
                {
                    playerInput.isInputEnabled = true;
                    DebugLogger.LogCutscene("Player input enabled");
                }
            });
            
            yield return new WaitForSeconds(0.5f); // Wait for fade to complete
            
            
            // Start ClockTimer NOW (after cutscene completes)
            if (clockTimer != null)
            {
                clockTimer.StartTimer(clockTimer.totalTime);
                DebugLogger.LogCutscene("Clock timer started after cutscene");
            }
            
            yield return new WaitForSeconds(2f); // Wait for HUD animation to play
            
            // SET DAY.ONE FLAG - This is the start of the game's day progression
            // This will automatically trigger a save because day flags auto-save
            if (!GameFlags.HasFlag("day.one"))
            {
                DebugLogger.LogCutscene("Setting day.one flag (game start)");
                GameFlags.SetFlag("day.one");
            }
            
            DebugLogger.LogCutscene("Complete");
            yield return null;
        }
        
        private void Awake()
        {
            // No longer need to set flags in Awake - we'll handle everything in PlayDaySpecificWakeUpDialogue
        }
        
        void Start()
        {
            DebugLogger.LogCutscene("Start() called");

            // Find main character components first
            var mainChar = GameObject.Find("MainCharacter");
            if (mainChar != null)
            {
                playerInput = mainChar.GetComponent<PlayerInput2D>();
                mainCharSpriteRenderer = mainChar.GetComponent<SpriteRenderer>();
                mainCharShadowCaster = mainChar.GetComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>();
                DebugLogger.LogCutscene("Found MainCharacter for input control, sprite renderer, and shadow caster");
            }
            else
            {
                DebugLogger.LogError("[OverworldWakeUpCutscene] MainCharacter not found!");
            }
            
            // Find MainCamera and CinemachineBrain
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
                DebugLogger.LogCutscene("Found MainCamera and CinemachineBrain");
            }
            else
            {
                DebugLogger.LogError("[OverworldWakeUpCutscene] MainCamera not found!");
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
                    DebugLogger.LogCutscene("Set SleepingMain sprite to sleeping");
                }
            }
            else
            {
                DebugLogger.LogError("[OverworldWakeUpCutscene] SleepingMain GameObject not assigned!");
            }
            
            // Find fade canvas
            fadeCanvasGroup = GameObject.Find("FadeCanvasGroup")?.GetComponent<CanvasGroup>();
            if (fadeCanvasGroup != null)
            {
                DebugLogger.LogCutscene("Found FadeCanvasGroup");
            }
            else
            {
                DebugLogger.LogError("[OverworldWakeUpCutscene] FadeCanvasGroup not found!");
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
                    DebugLogger.LogCutscene("Found EndTransition component");
                }
            }

            // Check if we should play the cutscene (via PlayerPrefs flag from TutorialScene)
            int playFlag = PlayerPrefs.GetInt("PlayWakeUpCutscene", 0);
            DebugLogger.LogCutscene($"PlayWakeUpCutscene flag value: {playFlag}");

            if (playFlag == 1)
            {
                // Clear the flag
                PlayerPrefs.SetInt("PlayWakeUpCutscene", 0);
                PlayerPrefs.Save();
                
                DebugLogger.LogCutscene("Setting up full wake-up cutscene...");
                
                // Set up ScreenFader with eyes ALREADY closed at start (player is waking up)
                ScreenFader screenFader = FindFirstObjectByType<ScreenFader>();
                if (screenFader != null)
                {
                    DebugLogger.LogCutscene("Setting up eyes closed position (player waking up)");
                    StartCoroutine(SetupEyesAlreadyClosedState(screenFader));
                }
                else
                {
                    // Fallback: use fade canvas if ScreenFader not available
                    if (fadeCanvasGroup != null)
                    {
                        fadeCanvasGroup.alpha = 1f; // Start opaque (black screen)
                        DebugLogger.LogCutscene("Fade canvas set to black (fallback)");
                    }
                    
                    DebugLogger.LogCutscene("Starting cutscene coroutine");
                    StartCoroutine(BeginWakeUpSequence());
                }
            }
            // Check if day.six flag is set - trigger ending instead of dialogue
            else if (GameFlags.HasFlag("day.six"))
            {
                DebugLogger.LogCutscene("day.six detected - triggering ending sequence");
                StartCoroutine(HandleDayEndingSequence("day.six"));
            }
            // NOTE: day.five does NOT trigger ending here - it triggers at END of day via ClockTimer
            // Check if we should play day-specific wake-up dialogue
            else if (ShouldPlayDaySpecificWakeUpDialogue())
            {
                string currentDay = GetCurrentDay();
                DebugLogger.LogCutscene($"Day-specific wake-up dialogue detected for {currentDay}, starting...");
                StartCoroutine(PlayDaySpecificWakeUpDialogue());
            }
            else
            {
                DebugLogger.LogCutscene("No cutscene to play, component will remain inactive");
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
            
            DebugLogger.LogCutscene("Eyes already closed state set up, starting cutscene");
            yield return null; // Wait one frame for everything to be set up
            StartCoroutine(BeginWakeUpSequence());
        }
        
        /// <summary>
        /// Call this method to trigger the full wake-up cutscene (day one initial wake up)
        /// </summary>
        public void TriggerFullWakeUpCutscene()
        {
            DebugLogger.LogCutscene("TriggerFullWakeUpCutscene() called");
            shouldPlayFullWakeUpCutscene = true;
        }
        
        /// <summary>
        /// Static method to trigger the wake-up cutscene from other scenes (via PlayerPrefs flag)
        /// </summary>
        public static void TriggerWakeUpCutscene()
        {
            DebugLogger.LogCutscene("TriggerWakeUpCutscene() static method called - setting PlayerPrefs flag");
            PlayerPrefs.SetInt("PlayWakeUpCutscene", 1);
            PlayerPrefs.Save();
            DebugLogger.LogCutscene($"Flag set to: {PlayerPrefs.GetInt("PlayWakeUpCutscene")}");
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
                        DebugLogger.LogCutscene($"{dayDialogue.dayFlag} wake-up dialogue available");
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
            DebugLogger.LogCutscene("Starting day-specific wake-up dialogue sequence");
            
            // Find clock timer
            if (clockTimer == null)
            {
                clockTimer = FindObjectOfType<ClockTimer>();
            }
            
            if (clockTimer == null)
            {
                DebugLogger.LogWarning("[OverworldWakeUpCutscene] ClockTimer not found - cannot continue");
                yield break;
            }

            // Get the current day to determine behavior
            string currentDay = GetCurrentDay();
            
            if (currentDay == "day.one")
            {
                // Day.one: Just start timer normally (no reconstruction needed)
                DebugLogger.LogCutscene("Day.one - starting timer normally without reconstruction");
                clockTimer.StartTimer(clockTimer.totalTime);
                yield break;
            }
            
            // Days 2-5: Call clock reconstruction directly (without starting timer)
            DebugLogger.LogCutscene($"{currentDay} detected - calling clock reconstruction");
            
            // Call the clock reconstruction coroutine directly and wait for it to complete
            yield return StartCoroutine(clockTimer.ReconstructClock());
            
            DebugLogger.LogCutscene("Clock reconstruction complete");
            
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
                    DebugLogger.LogCutscene("Found DialogBehaviour via FindFirstObjectByType");
                }
            }
            
            if (dialogueGraph == null)
            {
                DebugLogger.LogWarning("[OverworldWakeUpCutscene] Cannot play day-specific dialogue - dialogueGraph is null");
                
                // Start timer manually if dialogue won't play
                if (clockTimer != null)
                {
                    clockTimer.StartTimer(clockTimer.totalTime);
                    DebugLogger.LogCutscene("Started clock timer manually (no dialogue to play)");
                }
                yield break;
            }
            
            if (dialogBehaviour == null)
            {
                DebugLogger.LogWarning("[OverworldWakeUpCutscene] Cannot play day-specific dialogue - dialogBehaviour is null");
                
                // Start timer manually if dialogue won't play
                if (clockTimer != null)
                {
                    clockTimer.StartTimer(clockTimer.totalTime);
                    DebugLogger.LogCutscene("Started clock timer manually (no dialogBehaviour)");
                }
                yield break;
            }
            
            DebugLogger.LogCutscene($"Dialogue graph found: {dialogueGraph.name}, DialogBehaviour found: {dialogBehaviour.name}");

            // Set player dialogue state
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            CharacterMotor2D playerMotor = null;
            if (playerObj != null)
            {
                playerMotor = playerObj.GetComponent<CharacterMotor2D>();
                if (playerMotor != null)
                {
                    playerMotor.SetDialogueActive(true);
                    DebugLogger.LogCutscene("Player dialogue state set to active");
                }
            }

            // Wait for dialogue to finish
            bool dialogueFinished = false;
            UnityEngine.Events.UnityAction onFinished = () => { dialogueFinished = true; DebugLogger.LogCutscene("Dialogue finished callback triggered"); };
            dialogBehaviour.OnDialogFinished.AddListener(onFinished);
            
            // Also listen for when dialogue starts
            bool dialogueStarted = false;
            UnityEngine.Events.UnityAction onStarted = () => { dialogueStarted = true; DebugLogger.LogCutscene("Dialogue started callback triggered"); };
            dialogBehaviour.OnDialogStarted.AddListener(onStarted);
            
            DebugLogger.LogCutscene($"Starting day-specific wake-up dialogue: {dialogueGraph.name} (DialogBehaviour: {dialogBehaviour.name})");
            DebugLogger.LogCutscene($"DialogBehaviour enabled: {dialogBehaviour.enabled}, gameObject active: {dialogBehaviour.gameObject.activeInHierarchy}");
            
            try
            {
                dialogBehaviour.StartDialog(dialogueGraph);
                DebugLogger.LogCutscene("StartDialog() called successfully");
            }
            catch (System.Exception ex)
            {
                DebugLogger.LogError($"[OverworldWakeUpCutscene] Error starting dialogue: {ex.Message}\n{ex.StackTrace}");
            }
            
            // Wait a moment to see if dialogue starts
            yield return new WaitForSeconds(0.5f);
            if (!dialogueStarted)
            {
                DebugLogger.LogWarning("[OverworldWakeUpCutscene] Dialogue did not start within 0.5 seconds - may be an issue with dialogue system");
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
                DebugLogger.LogCutscene("Clock timer started after day-specific dialogue completed");
            }
            else
            {
                DebugLogger.LogWarning("[OverworldWakeUpCutscene] ClockTimer not found - cannot start timer!");
            }

            DebugLogger.LogCutscene("Day-specific wake-up dialogue complete");
        }
        
        /// <summary>
        /// Handle the special day ending sequence (day.five or day.six)
        /// </summary>
        private IEnumerator HandleDayEndingSequence(string dayFlag)
        {
            DebugLogger.LogCutscene($"Starting ending sequence for {dayFlag}");
            
            // Pause the environment using GlobalPause
            GlobalPause.SetMinigamePaused(true);
            DebugLogger.LogCutscene("Environment paused for ending sequence");
            
            // Optional: Add a brief delay before triggering ending
            yield return new WaitForSeconds(1f);
            
            // Set the start.ending flag to trigger the ending
            if (!GameFlags.HasFlag("start.ending"))
            {
                DebugLogger.LogCutscene($"Setting start.ending flag for {dayFlag}");
                GameFlags.SetFlag("start.ending");
            }
            
            // Trigger the ending transition
            if (endTransition != null)
            {
                DebugLogger.LogCutscene($"Triggering EndTransition for {dayFlag}");
                endTransition.TriggerEndTransition();
            }
            else
            {
                DebugLogger.LogError("[OverworldWakeUpCutscene] EndTransition component not found! Cannot trigger ending.");
                
                // Fallback: unpause if we can't trigger ending
                GlobalPause.SetMinigamePaused(false);
            }
            
            DebugLogger.LogCutscene($"Ending sequence initiated for {dayFlag}");
        }
        
        /// <summary>
        /// Check for evidence flags and set corresponding card flags for the ending.
        /// This runs every day (days 2-5) so cards are unlocked as evidence is collected.
        /// </summary>
        private void CheckAndSetEvidenceCardFlags()
        {
            DebugLogger.LogCutscene("Checking evidence flags for card unlocks...");
            
            // evidence.knife -> card.shield_slash
            if (GameFlags.HasFlag("evidence.knife"))
            {
                if (!GameFlags.HasFlag("card.shield_slash"))
                {
                    GameFlags.SetFlag("card.shield_slash");
                    DebugLogger.LogCutscene("Evidence: knife found -> Unlocked card: shield_slash");
                }
                else
                {
                    DebugLogger.LogCutscene("Evidence: knife found (card.shield_slash already unlocked)");
                }
            }
            
            // evidence.throne -> card.dramatic_exit
            if (GameFlags.HasFlag("evidence.throne"))
            {
                if (!GameFlags.HasFlag("card.dramatic_exit"))
                {
                    GameFlags.SetFlag("card.dramatic_exit");
                    DebugLogger.LogCutscene("Evidence: throne found -> Unlocked card: dramatic_exit");
                }
                else
                {
                    DebugLogger.LogCutscene("Evidence: throne found (card.dramatic_exit already unlocked)");
                }
            }
            
            // evidence.silverware -> card.tariff_strike
            if (GameFlags.HasFlag("evidence.silverware"))
            {
                if (!GameFlags.HasFlag("card.tariff_strike"))
                {
                    GameFlags.SetFlag("card.tariff_strike");
                    DebugLogger.LogCutscene("Evidence: silverware found -> Unlocked card: tariff_strike");
                }
                else
                {
                    DebugLogger.LogCutscene("Evidence: silverware found (card.tariff_strike already unlocked)");
                }
            }
            
            DebugLogger.LogCutscene("Evidence flag check complete");
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
                DebugLogger.LogWarning("[OverworldWakeUpCutscene] ClockTimer not found - cannot apply battle result time adjustment");
                return;
            }
            
            bool timeWasAdjusted = false;
            
            // Check for win flag
            if (GameFlags.HasFlag("nether.win"))
            {
                DebugLogger.LogCutscene($"? Battle won! Adding {winTimeBonus} seconds to clock timer");
                clockTimer.AddTime(winTimeBonus);
                GameFlags.RemoveFlag("nether.win");
                DebugLogger.LogCutscene("Cleared nether.win flag");
                timeWasAdjusted = true;
            }
            // Check for lose flag
            else if (GameFlags.HasFlag("nether.lose"))
            {
                DebugLogger.LogCutscene($"? Battle lost! Subtracting {loseTimePenalty} seconds from clock timer");
                clockTimer.RemoveTime(loseTimePenalty);
                GameFlags.RemoveFlag("nether.lose");
                DebugLogger.LogCutscene("Cleared nether.lose flag");
                timeWasAdjusted = true;
            }
            else
            {
                DebugLogger.LogCutscene("No battle result flags detected (nether.win or nether.lose)");
            }
            
            // Save the game if time was adjusted to persist the new clock time
            if (timeWasAdjusted)
            {
                DebugLogger.LogCutscene("Saving game after clock time adjustment...");
                GameFlagsManager.SaveCurrentGame();
                DebugLogger.LogCutscene("Game saved successfully with new clock time");
            }
        }
    }
}