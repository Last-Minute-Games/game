using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (battleSystem == null)
            battleSystem = FindObjectOfType<BattleSystem>();

        if (button != null)
            button.onClick.AddListener(OnEndTurnClicked);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnEndTurnClicked);
    }

    private void OnEndTurnClicked()
    {
        if (battleSystem == null)
        {
            Debug.LogWarning("⚠️ EndTurnButton: BattleSystem reference missing!");
            return;
        }

        Debug.Log("⏩ Player skipped their turn.");
        button.interactable = false;
        battleSystem.EndPlayerTurn();
    }

    // called at start of player's turn
    public void EnableButton()
    {
        gameObject.SetActive(true);
        button.interactable = true;
        Debug.Log("🟢 End Turn Button re-enabled for player.");
    }

    // called when player turn ends
    public void DisableButton()
    {
        button.interactable = false;
        gameObject.SetActive(true); // ensure it stays visible, just dimmed
    }
}
