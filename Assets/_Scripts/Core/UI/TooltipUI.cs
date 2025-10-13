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

    public void Show(CardData data)
    {
        if (data == null) return;

        titleText.text = data.cardName;
        descriptionText.text = data.description;
        // use energyCost instead of energy (CardData uses energyCost)
        statsText.text = $"Type: {data.cardType}\nCost: {data.energyCost}";

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    void Update()
    {
        // Follow mouse position
        if (gameObject.activeSelf)
        {
            Vector2 pos = Input.mousePosition;
            rectTransform.position = pos + new Vector2(120f, -80f);
        }
    }
}
