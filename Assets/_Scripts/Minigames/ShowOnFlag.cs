using UnityEngine;

public class ShowOnFlag : MonoBehaviour
{
    [SerializeField] private string flagName = "minigame.sokoban.show";
    [Tooltip("If empty, this GameObject is toggled.")]
    [SerializeField] private GameObject[] objectsToToggle;

    private void Awake()
    {
        // Default to toggling ourselves
        if (objectsToToggle == null || objectsToToggle.Length == 0)
        {
            objectsToToggle = new[] { gameObject };
        }

        // Subscribe to GameFlags events
        if (GameFlags.Instance != null)
        {
            GameFlags.Instance.OnInitialized += HandleFlagsInitialized;
            GameFlags.Instance.OnFlagChanged += HandleFlagChanged;
        }

        // Initial check in Awake
        UpdateVisibility();
    }

    private void Start()
    {
        // Check again in Start to ensure GameFlags is fully initialized
        // This handles cases where GameFlags wasn't ready in Awake
        UpdateVisibility();
    }

    private void OnDestroy()
    {
        if (GameFlags.Instance != null)
        {
            GameFlags.Instance.OnInitialized -= HandleFlagsInitialized;
            GameFlags.Instance.OnFlagChanged -= HandleFlagChanged;
        }
    }

    private void HandleFlagsInitialized()
    {
        UpdateVisibility();
    }

    private void HandleFlagChanged(string changedFlag)
    {
        if (changedFlag == flagName)
        {
            UpdateVisibility();
        }
    }

    private void UpdateVisibility()
    {
        // Always show when flag exists, hide when it doesn't
        bool hasFlag = GameFlags.HasFlag(flagName);

        foreach (var obj in objectsToToggle)
        {
            if (obj != null)
            {
                obj.SetActive(hasFlag);
            }
        }
    }
}
