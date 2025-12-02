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
        
        private bool hasPlayed = false;
        
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
        
        private void Awake()
        {
            // Check early if we should play day-specific wake-up dialogue
            // We need to set the flag BEFORE ClockTimer.Start() runs to prevent it from starting the timer
            if (ShouldPlayDaySpecificWakeUpDialogue())
            {
                Debug.Log("[OverworldWakeUpCutscene] Day-specific wake-up dialogue detected in Awake(), setting flag to skip timer start");
                // Set flag to prevent ClockTimer from starting after reconstruction
                PlayerPrefs.SetInt("SkipClockTimerStartAfterReconstruct", 1);
                PlayerPrefs.Save();
            }
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

            // CHECK FOR BATTLE RESULT FLAGS AND ADJUST CLOCK TIMER
            // This happens before any cutscene logic to ensure time is adjusted properly
            CheckAndApplyBattleResultTimeAdjustment();

            // Check if we should play the cutscene
            int playFlag = PlayerPrefs.GetInt("PlayWakeUpCutscene", 0);
            Debug.Log($"[OverworldWakeUpCutscene] Flag value: {playFlag}");

            if (playFlag != 1)
            {
                // Check if we should play day-specific wake-up dialogue instead
                if (ShouldPlayDaySpecificWakeUpDialogue())
                {
                    Debug.Log("[OverworldWakeUpCutscene] Day-specific wake-up dialogue detected, starting...");
                    StartCoroutine(PlayDaySpecificWakeUpDialogue());
                }
                else
                {
                    Debug.Log("[OverworldWakeUpCutscene] Flag not set and no day-specific dialogue, disabling");
                    enabled = false;
                }
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
            
            // Set flag to prevent clock re-animate animation during cutscene
            PlayerPrefs.SetInt("SkipClockReanimate", 1);
            Debug.Log("[OverworldWakeUpCutscene] Set flag to skip clock re-animate animation");
            
            PlayerPrefs.SetInt("PlayWakeUpCutscene", 1);
            PlayerPrefs.Save();
            Debug.Log($"[OverworldWakeUpCutscene] Flag set to: {PlayerPrefs.GetInt("PlayWakeUpCutscene")}" +
                      $" [next scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1}]");
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
                case "day.five": return null; // No day after five
                default: return null;
            }
        }
        
        /// <summary>
        /// Play day-specific wake-up dialogue after clock reconstruction completes
        /// </summary>
        private IEnumerator PlayDaySpecificWakeUpDialogue()
        {
            Debug.Log("[OverworldWakeUpCutscene] Starting day-specific wake-up dialogue sequence");
            
            // Flag was already set in Awake(), ClockTimer will check it after reconstruction
            
            // Find clock timer
            if (clockTimer == null)
            {
                clockTimer = FindObjectOfType<ClockTimer>();
            }
            
            // Wait for clock reconstruction animation to complete
            // ClockTimer will automatically start reconstruction if HUD was shown before
            // We detect completion by checking when the SkipClockTimerStartAfterReconstruct flag gets cleared
            Debug.Log("[OverworldWakeUpCutscene] Waiting for clock reconstruction to complete...");
            
            // Wait until the flag is cleared (which happens when reconstruction finishes)
            float waitTimeout = 10f; // Safety timeout
            float elapsed = 0f;
            bool reconstructionComplete = false;
            
            while (elapsed < waitTimeout && !reconstructionComplete)
            {
                // Check if the flag was cleared (reconstruction finished)
                int flagValue = PlayerPrefs.GetInt("SkipClockTimerStartAfterReconstruct", 0);
                if (flagValue == 0)
                {
                    // Flag was cleared, reconstruction is complete
                    reconstructionComplete = true;
                    Debug.Log("[OverworldWakeUpCutscene] Clock reconstruction complete!");
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (!reconstructionComplete)
            {
                Debug.LogWarning("[OverworldWakeUpCutscene] Clock reconstruction wait timeout - proceeding anyway");
            }
            
            // Small delay after reconstruction to let things settle
            yield return new WaitForSeconds(0.3f);

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
                    Debug.Log("[OverworldWakeUpCutscene] Started clock timer manually (no dialogue to play)");
                }
                yield break;
            }
            
            Debug.Log($"[OverworldWakeUpCutscene] Dialogue graph found: {dialogueGraph.name}, DialogBehaviour found: {dialogBehaviour.name}");

            // Clock timer should not have started yet (we set the flag), but ensure it's paused
            if (clockTimer != null)
            {
                clockTimer.PauseTimer(true);
                Debug.Log("[OverworldWakeUpCutscene] Clock timer paused for day-specific dialogue (should already be stopped)");
            }

            // Set player dialogue state
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                CharacterMotor2D playerMotor = playerObj.GetComponent<CharacterMotor2D>();
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
            if (playerObj != null)
            {
                CharacterMotor2D playerMotor = playerObj.GetComponent<CharacterMotor2D>();
                if (playerMotor != null)
                {
                    playerMotor.SetDialogueActive(false);
                }
            }

            // NOW start the clock timer (it hasn't started yet)
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