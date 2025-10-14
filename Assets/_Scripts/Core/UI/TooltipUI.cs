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

    public void Show(CardData data, CardRunner runner, CharacterBase viewer)
    {
        if (data == null) return;

        string title = data.cardName;      // no mutation
        string desc  = data.description;   // no mutation

        // Build <X> with the SAME roll + viewer scale
        if (runner != null && viewer != null && !string.IsNullOrEmpty(desc) && desc.Contains("<X>"))
        {
            // Lock roll on first show (if not already rolled)
            runner.RollIfNeeded(data);

            int x = runner.GetPreviewX(viewer);

            // Color by effect kind (same heuristic as enemy)
            string colorHex = "#FFCF40";
            if (data.effects != null)
            {
                foreach (var eff in data.effects)
                {
                    if (eff is DamageEffect) colorHex = "#FF4040";
                    else if (eff is HealEffect) colorHex = "#40FF70";
                    else if (eff is BlockEffect) colorHex = "#40BFFF";
                }
            }
            string coloredX = $"<color={colorHex}>{x}</color>";
            desc = desc.Replace("<X>", coloredX);
        }

        titleText.text = title;
        descriptionText.text = desc;
        statsText.text = $"Type: {data.cardType}\nCost: {data.energy}";

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
