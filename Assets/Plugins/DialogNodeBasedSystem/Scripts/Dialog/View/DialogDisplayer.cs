using UnityEngine;

using UnityEngine;

namespace cherrydev
{
    public class DialogDisplayer : MonoBehaviour
    {
        [Header("MAIN COMPONENT")]
        [SerializeField] private DialogBehaviour _dialogBehaviour;

        [Header("NODE PANELS")]
        [SerializeField] private SentencePanel _dialogSentencePanel;
        [SerializeField] private AnswerPanel _dialogAnswerPanel;
        [SerializeField] private AnswerPanel _characterSelectionPanel; // NEW: Second panel

        private AnswerPanel _currentAnswerPanel; // NEW: Track which panel is active
        
        // Static cooldown to prevent immediately re-entering dialog after closing
        private static float _dialogExitCooldown = 0f;
        private const float DIALOG_EXIT_COOLDOWN_TIME = 0.3f;

        // Track when dialog was started to prevent immediate exit
        private float _dialogStartTime = 0f;
        private const float DIALOG_START_GRACE_PERIOD = 0.1f; // Don't allow exit for 0.1s after start

        public static bool IsInDialogExitCooldown => Time.unscaledTime < _dialogExitCooldown;

        private void Update()
        {
            // Check if any dialog panel is active
            bool isDialogActive = (_dialogSentencePanel != null && _dialogSentencePanel.gameObject.activeSelf) ||
                                  (_dialogAnswerPanel != null && _dialogAnswerPanel.gameObject.activeSelf) ||
                                  (_characterSelectionPanel != null && _characterSelectionPanel.gameObject.activeSelf);

            if (!isDialogActive)
                return;

            // Don't allow exit if dialog just started (prevents same E key press from closing it)
            if (Time.unscaledTime < _dialogStartTime + DIALOG_START_GRACE_PERIOD)
                return;

            // Exit dialog on E key press or right-click (only if allowed)
            if (_dialogBehaviour.CanExitDialog && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1)))
            {
                // Stop any playing text sound
                if (_dialogSentencePanel != null)
                {
                    _dialogSentencePanel.StopTextSound();
                }

                _dialogBehaviour.ForceEndDialog();
                // Set cooldown to prevent immediately re-entering dialog
                _dialogExitCooldown = Time.unscaledTime + DIALOG_EXIT_COOLDOWN_TIME;
            }
        }

        private void Awake()
        {
            Debug.Log($"[DialogDisplayer] Awake called on '{gameObject.name}'");
            Debug.Log($"[DialogDisplayer] GameObject active: {gameObject.activeInHierarchy}");
            Debug.Log($"[DialogDisplayer] Component enabled: {enabled}");
            Debug.Log($"[DialogDisplayer] _dialogBehaviour reference: {(_dialogBehaviour != null ? _dialogBehaviour.gameObject.name : "NULL!")}");
            Debug.Log($"[DialogDisplayer] _dialogSentencePanel: {(_dialogSentencePanel != null ? "OK" : "NULL")}");
            Debug.Log($"[DialogDisplayer] _dialogAnswerPanel: {(_dialogAnswerPanel != null ? "OK" : "NULL")}");
        }

        private void OnEnable()
        {
            // DIAGNOSTIC: Check if we're properly connected
            if (_dialogBehaviour == null)
            {
                Debug.LogError("[DialogDisplayer] CRITICAL: _dialogBehaviour is NULL! Dialog UI will not show. Please assign DialogBehaviour in the Inspector.");
                return;
            }

            Debug.Log($"[DialogDisplayer] OnEnable - Connected to DialogBehaviour on '{_dialogBehaviour.gameObject.name}'");

            _dialogBehaviour.AddListenerToDialogFinishedEvent(DisableDialogPanel);

            _dialogBehaviour.DialogDisabled += DisableDialogPanel;
            _dialogBehaviour.AnswerButtonSetUp += SetUpAnswerButtonsClickEvent;

            _dialogBehaviour.DialogTextCharWrote += _dialogSentencePanel.IncreaseMaxVisibleCharacters;
            _dialogBehaviour.DialogTextSkipped += _dialogSentencePanel.ShowFullDialogText;

            _dialogBehaviour.SentenceNodeActivated += EnableDialogSentencePanel;
            _dialogBehaviour.SentenceNodeActivated += DisableAllAnswerPanels;
            _dialogBehaviour.SentenceNodeActivated += _dialogSentencePanel.ResetDialogText;
            _dialogBehaviour.SentenceNodeActivatedWithParameter += _dialogSentencePanel.Setup;

            _dialogBehaviour.AnswerNodeActivated += EnableDialogAnswerPanel;
            _dialogBehaviour.AnswerNodeActivated += DisableDialogSentencePanel;

            _dialogBehaviour.MaxAmountOfAnswerButtonsCalculated += SetUpAllAnswerPanelButtons;

            _dialogBehaviour.AnswerNodeSetUp += SetUpAnswerDialogPanel;
#if UNITY_LOCALIZATION
            _dialogBehaviour.LanguageChanged += HandleLanguageChanged;
#endif

            Debug.Log($"[DialogDisplayer] Successfully subscribed to all DialogBehaviour events");
        }

        private void OnDisable()
        {
            _dialogBehaviour.DialogDisabled -= DisableDialogPanel;
            _dialogBehaviour.AnswerButtonSetUp -= SetUpAnswerButtonsClickEvent;

            _dialogBehaviour.DialogTextCharWrote -= _dialogSentencePanel.IncreaseMaxVisibleCharacters;
            _dialogBehaviour.DialogTextSkipped -= _dialogSentencePanel.ShowFullDialogText;

            _dialogBehaviour.SentenceNodeActivated -= EnableDialogSentencePanel;
            _dialogBehaviour.SentenceNodeActivated -= DisableAllAnswerPanels;
            _dialogBehaviour.SentenceNodeActivated += _dialogSentencePanel.ResetDialogText;
            _dialogBehaviour.SentenceNodeActivatedWithParameter -= _dialogSentencePanel.Setup;

            _dialogBehaviour.AnswerNodeActivated -= EnableDialogAnswerPanel;
            _dialogBehaviour.AnswerNodeActivated -= DisableDialogSentencePanel;

            _dialogBehaviour.MaxAmountOfAnswerButtonsCalculated -= SetUpAllAnswerPanelButtons;

            _dialogBehaviour.AnswerNodeSetUp -= SetUpAnswerDialogPanel;
#if UNITY_LOCALIZATION
            _dialogBehaviour.LanguageChanged -= HandleLanguageChanged;
#endif
        }

        /// <summary>
        /// Disable dialog answer and sentence panel
        /// </summary>
        public void DisableDialogPanel()
        {
            DisableAllAnswerPanels();
            DisableDialogSentencePanel();
        }

        /// <summary>
        /// Enable the appropriate dialog answer panel based on the current answer node
        /// </summary>
        public void EnableDialogAnswerPanel()
        {
            AnswerNode currentAnswerNode = _dialogBehaviour.CurrentAnswerNode;
            
            if (currentAnswerNode == null)
            {
                Debug.LogWarning("No current answer node found");
                return;
            }

            // Determine which panel to use based on the node's PanelType
            _currentAnswerPanel = currentAnswerNode.PanelType switch
            {
                AnswerPanelType.CharacterSelection => _characterSelectionPanel,
                _ => _dialogAnswerPanel
            };

            // Disable all panels first
            DisableAllAnswerPanels();

            // Enable and setup the selected panel
            ActiveGameObject(_currentAnswerPanel.gameObject, true);
            _currentAnswerPanel.DisableAllButtons();
            _currentAnswerPanel.EnableCertainAmountOfButtons(currentAnswerNode.Answers.Count);
        }

        /// <summary>
        /// Disable all answer panels
        /// </summary>
        private void DisableAllAnswerPanels()
        {
            ActiveGameObject(_dialogAnswerPanel.gameObject, false);
            if (_characterSelectionPanel != null)
                ActiveGameObject(_characterSelectionPanel.gameObject, false);
        }

        /// <summary>
        /// Enable dialog sentence panel
        /// </summary>
        public void EnableDialogSentencePanel()
        {
            Debug.Log($"[DialogDisplayer] EnableDialogSentencePanel called!");
            Debug.Log($"[DialogDisplayer] _dialogSentencePanel null? {_dialogSentencePanel == null}");

            if (_dialogSentencePanel == null)
            {
                Debug.LogError("[DialogDisplayer] CRITICAL: _dialogSentencePanel is NULL! Cannot show dialog!");
                return;
            }

            // Mark when dialog started to prevent immediate exit
            _dialogStartTime = Time.unscaledTime;

            _dialogSentencePanel.ResetDialogText();
            ActiveGameObject(_dialogSentencePanel.gameObject, true);
            Debug.Log($"[DialogDisplayer] Sentence panel activated: {_dialogSentencePanel.gameObject.name}");
        }

        /// <summary>
        /// Disable dialog sentence panel
        /// </summary>
        public void DisableDialogSentencePanel() => ActiveGameObject(_dialogSentencePanel.gameObject, false);

        /// <summary>
        /// Enable or disable game object depends on isActive bool flag
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="isActive"></param>
        public void ActiveGameObject(GameObject gameObject, bool isActive)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("Game object is null");
                return;
            }

            gameObject.SetActive(isActive);
        }
        
        /// <summary>
        /// Removing all listeners and Setting up answer button onClick event
        /// </summary>
        /// <param name="index"></param>
        /// <param name="answerNode"></param>
        public void SetUpAnswerButtonsClickEvent(int index, AnswerNode answerNode)
        {
            if (_currentAnswerPanel == null)
            {
                Debug.LogWarning("No current answer panel set");
                return;
            }

            _currentAnswerPanel.GetButtonByIndex(index).onClick.RemoveAllListeners();
            _currentAnswerPanel.AddButtonOnClickListener(index, 
                () => _dialogBehaviour.SetCurrentNodeAndHandleDialogGraph(index));
        }

        /// <summary>
        /// Setting up answer dialog panel
        /// </summary>
        /// <param name="index"></param>
        /// <param name="answerText"></param>
        public void SetUpAnswerDialogPanel(int index, string answerText)
        {
            if (_currentAnswerPanel == null)
            {
                Debug.LogWarning("No current answer panel set");
                return;
            }

            AnswerNode currentAnswerNode = _dialogBehaviour.CurrentAnswerNode;
            
            if (currentAnswerNode != null)
                _currentAnswerPanel.GetButtonTextByIndex(index).text = currentAnswerNode.GetAnswerText(index);
            else
                _currentAnswerPanel.GetButtonTextByIndex(index).text = answerText;
        }

        /// <summary>
        /// Setup buttons for all answer panels
        /// </summary>
        /// <param name="maxAmountOfAnswerButtons"></param>
        private void SetUpAllAnswerPanelButtons(int maxAmountOfAnswerButtons)
        {
            _dialogAnswerPanel.SetUpButtons(maxAmountOfAnswerButtons);
            if (_characterSelectionPanel != null)
                _characterSelectionPanel.SetUpButtons(maxAmountOfAnswerButtons);
        }

        private void HandleLanguageChanged()
        {
            if (_dialogBehaviour.CurrentAnswerNode != null)
                RefreshAnswerButtons();
        }
        
        /// <summary>
        /// Refresh all answer buttons with updated localized text
        /// </summary>
        private void RefreshAnswerButtons()
        {
            if (_currentAnswerPanel == null)
                return;

            AnswerNode currentAnswerNode = _dialogBehaviour.CurrentAnswerNode;
            
            if (currentAnswerNode != null)
            {
                for (int i = 0; i < currentAnswerNode.Answers.Count; i++)
                {
                    if (i < _currentAnswerPanel.GetButtonCount() &&
                        _currentAnswerPanel.GetButtonByIndex(i).gameObject.activeSelf)
                        _currentAnswerPanel.GetButtonTextByIndex(i).text = currentAnswerNode.GetAnswerText(i);
                }
            }
        }
    }
}