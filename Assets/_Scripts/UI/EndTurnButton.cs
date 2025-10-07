using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private Button button;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (battleSystem == null)
            battleSystem = FindObjectOfType<BattleSystem>();

        if (button != null)
            button.onClick.AddListener(OnEndTurnClicked);

        // 🔹 CanvasGroup lets us fade + disable interaction safely
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
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

        Debug.Log("⏩ Player ended turn.");

        DisableButton(); // 🔹 immediately disable interaction
        battleSystem.EndPlayerTurn();
    }

    // ✅ Fully enable
    public void EnableButton()
    {
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
        button.interactable = true;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 🔹 Fix: re-enable any parent canvas group that may have been disabled
        CanvasGroup parentGroup = GetComponentInParent<CanvasGroup>();
        if (parentGroup != null)
        {
            parentGroup.alpha = 1f;
            parentGroup.blocksRaycasts = true;
        }

        Debug.Log("🟢 End Turn Button ENABLED (fully interactive again).");
    }

    // 🔒 Visually dim & block input
    public void DisableButton()
    {
        button.interactable = false;
        canvasGroup.alpha = 0.5f;
        canvasGroup.blocksRaycasts = false;
        Debug.Log("🔴 End Turn Button DISABLED");
    }
}
