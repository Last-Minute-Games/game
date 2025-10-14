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

        string title = data.cardName;      // original title
        string desc  = data.description;   // no mutation

        // ===========================
        //  Determine Roll Label ("Poor", "Potent", etc.)
        // ===========================
        if (runner != null)
        {
            runner.RollIfNeeded(data); // ensure it has a roll
            float roll = runner.randomScale;

            float range = data.maxMultiplier - data.minMultiplier;
            if (range > 0f)
            {
                float bottomThreshold = data.minMultiplier + range / 3f;
                float topThreshold    = data.maxMultiplier - range / 3f;

                if (roll <= bottomThreshold)
                {
                    title = $"<color=#6A6A6A>Poor</color> {data.cardName}";
                }
                else if (roll >= topThreshold)
                {
                    title = $"<color=#FFD700>Potent</color> {data.cardName}";
                }
                // else: keep title normal
            }
        }

        // ===========================
        //   Build Description (<X> values)
        // ===========================
        if (runner != null && viewer != null && !string.IsNullOrEmpty(desc) && desc.Contains("<X>"))
        {
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

        // ===========================
        //   Display in Tooltip
        // ===========================
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
