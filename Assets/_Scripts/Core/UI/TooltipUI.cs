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
        if (data == null) return;

        // ============ BASE VALUES ============
        float potency = runner ? runner.cachedPotency : 1f;

        // Determine prefix dynamically (same logic as CardEffect)
        float range = data.maxMultiplier - data.minMultiplier;
        string prefix = "";
        if (range > 0.5f)
        {
            float normalized = (potency / (data.maxMultiplier * (runner ? (runner.owner is Enemy e ? e.globalPowerScale : 1f) : 1f)));
            if (normalized < 0.33f) prefix = "Poor ";
            else if (normalized > 0.66f) prefix = "Potent ";
        }

        string displayName = prefix + data.cardName.Split(' ')[^1];

        // Compute substituted text
        string desc = data.description;
        string intent = data.intentionText;
        string coloredValue = "<color=#FFCF40>";

        if (runner != null)
        {
            // Approximate potency-adjusted value if effect provides it
            float effectiveValue = 0f;

            if (data.effects != null)
            {
                foreach (var eff in data.effects)
                {
                    if (eff is DamageEffect dmg)
                        effectiveValue = dmg.baseDamage * potency;
                    else if (eff is HealEffect heal)
                        effectiveValue = heal.baseHeal * potency;
                    else if (eff is BlockEffect blk)
                        effectiveValue = blk.baseBlock * potency;
                }
            }

            coloredValue += $"{effectiveValue:F0}</color>";

            if (!string.IsNullOrEmpty(desc) && desc.Contains("<X>"))
                desc = desc.Replace("<X>", coloredValue);

            if (!string.IsNullOrEmpty(intent) && intent.Contains("<X>"))
                intent = intent.Replace("<X>", coloredValue);
        }

        // ============ APPLY TO UI ============
        titleText.text = displayName;
        descriptionText.text = desc;
        statsText.text = $"Type: {data.cardType}\nCost: {data.energy}";

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(true);

        Debug.Log($"🟨 Tooltip updated: {displayName} | potency={potency:F2}");
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
