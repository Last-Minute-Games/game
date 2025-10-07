using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    [Header("References")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text statsText;
    public CanvasGroup canvasGroup;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        Hide();
    }

    public void Show(CardBase card)
    {
        if (card == null) return;

        titleText.text = card.cardName;
        descriptionText.text = card.description;
        statsText.text = $"Type: {card.cardType}\nCost: {card.energy}";

        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }

    void Update()
    {
        // Follow mouse
        if (gameObject.activeSelf)
        {
            Vector2 pos = Input.mousePosition;
            rectTransform.position = pos + new Vector2(120f, -80f);
        }
    }
}
