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

    public void Show(CardData data, CardRunner runner = null)
    {
        if (data == null)
        {
            Hide();
            return;
        }

        titleText.text = data.cardName;
        descriptionText.text = data.description;

        string stats = $"Type: {data.cardType}\nCost: {data.energy}";

        // --- Add potency if runner provided ---
        if (runner != null)
        {
            float potency = runner.cachedPotency;

            // Color-coded potency text
            Color potColor = potency > 1f ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.6f, 0.6f);
            string hex = ColorUtility.ToHtmlStringRGB(potColor);
            stats += $"\n<color=#{hex}>Potency: ×{potency:F2}</color>";
        }

        statsText.text = stats;

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
        if (gameObject.activeSelf)
        {
            Vector2 pos = Input.mousePosition;
            rectTransform.position = pos + new Vector2(120f, -80f);
        }
    }
}
