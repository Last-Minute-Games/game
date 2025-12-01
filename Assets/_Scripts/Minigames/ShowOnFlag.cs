using UnityEngine;

public class ShowOnFlag : MonoBehaviour
{
    [SerializeField] private string flagName = "minigame.sokoban.show";
    [Tooltip("If empty, this GameObject is toggled.")]
    [SerializeField] private GameObject[] objectsToToggle;

    [Tooltip("If true: visible when flag exists. If false: visible when flag does NOT exist.")]
    [SerializeField] private bool visibleWhenFlagSet = true;

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

        // In case GameFlags is already initialized, update now
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
        bool hasFlag = GameFlags.HasFlag(flagName);
        bool shouldBeActive = visibleWhenFlagSet ? hasFlag : !hasFlag;

        foreach (var obj in objectsToToggle)
        {
            if (obj != null)
            {
                obj.SetActive(shouldBeActive);
            }
        }
    }
}
