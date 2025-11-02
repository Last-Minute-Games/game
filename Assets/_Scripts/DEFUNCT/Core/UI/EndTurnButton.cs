using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class EndTurnButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleSystem battleSystem;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (battleSystem == null)
            battleSystem = FindObjectOfType<BattleSystem>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnEndTurnClicked);

        // ensure fully visible and interactable always
        var cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
        }

        button.interactable = true;
    }

    private void OnEndTurnClicked()
    {
        if (battleSystem == null)
        {
            Debug.LogWarning("⚠️ EndTurnButton: BattleSystem reference missing!");
            return;
        }

        Debug.Log("⏩ Player ended turn (button always active).");
        battleSystem.EndPlayerTurn();
    }
}
