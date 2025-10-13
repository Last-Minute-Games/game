using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private GameFlags gameFlags;
    [SerializeField] private JournalManager journalManager;

    private void Awake()
    {
        if (journalManager && gameFlags)
        {
            journalManager.Hook(gameFlags);
            Debug.Log("[GameInitializer] Journal hooked to GameFlags.");
        }
        else
        {
            Debug.LogWarning("[GameInitializer] Missing references!");
        }
    }
}
