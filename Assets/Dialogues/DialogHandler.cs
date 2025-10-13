using cherrydev;
using UnityEngine;

namespace Dialogues
{
    public class DialogTrigger : MonoBehaviour
    {
        [Header("Dialog Settings")]
        [SerializeField] private DialogBehaviour dialogBehaviour; // likely shared UI
        [SerializeField] private DialogNodeGraph dialogGraph;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("Detection Settings")]
        private readonly float _interactionRange = 1f;

        private NpcBrain2D _npcBrain;
        private NpcBrain2D.NpcMode _previousMode;

        private GameObject _player;
        private CharacterMotor2D _playerController;

        private bool _isPlayerNear = false;
        private bool _isMyConversation = false; // <-- key flag

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _playerController = _player.GetComponent<CharacterMotor2D>();

            _npcBrain = GetComponent<NpcBrain2D>();

            dialogBehaviour.OnDialogStarted.AddListener(OnDialogStart);
            dialogBehaviour.OnDialogFinished.AddListener(OnDialogFinished);
        }

        private void OnDestroy()
        {
            // Always unsubscribe to prevent ghost callbacks in the editor
            if (dialogBehaviour)
            {
                dialogBehaviour.OnDialogStarted.RemoveListener(OnDialogStart);
                dialogBehaviour.OnDialogFinished.RemoveListener(OnDialogFinished);
            }
        }

        private void OnDialogStart()
        {
            // Ignore global start events unless they were initiated by THIS trigger
            if (!_isMyConversation) return;

            if (_npcBrain)
            {
                _previousMode = _npcBrain.mode;
                _npcBrain.mode = NpcBrain2D.NpcMode.Idle;
            }

            if (_playerController) _playerController.SetDialogueActive(true);
        }

        private void OnDialogFinished()
        {
            // Only unfreeze if this NPC was the one talking
            if (!_isMyConversation) return;

            if (_npcBrain)
            {
                _npcBrain.mode = _previousMode;
            }

            if (_playerController) _playerController.SetDialogueActive(false);

            _isMyConversation = false; // reset
        }

        void Update()
        {
            if (_playerController && _playerController.IsDialogueActive) return;

            if (_player)
                _isPlayerNear = Vector3.Distance(transform.position, _player.transform.position) <= _interactionRange;

            if (_isPlayerNear && Input.GetKeyDown(interactKey))
            {
                StartDialogue();
            }
        }

        private void StartDialogue()
        {
            if (dialogBehaviour && dialogGraph)
            {
                // Mark that the NEXT OnDialogStarted/Finished belongs to THIS NPC
                _isMyConversation = true;
                dialogBehaviour.StartDialog(dialogGraph);
            }
            else
            {
                Debug.LogWarning("DialogTrigger: Missing DialogBehaviour or DialogNodeGraph reference.");
            }
        }
    }
}
