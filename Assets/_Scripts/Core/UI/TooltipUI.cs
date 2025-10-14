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

        // === 1️⃣ Predict potency (simulate preview roll)
        float randomScale = Random.Range(data.minMultiplier, data.maxMultiplier);
        float globalScale = 1f;

        if (runner != null && runner.owner != null)
        {
            if (runner.owner is Enemy e)
                globalScale = e.globalPowerScale;
            else if (runner.owner is Player p)
                globalScale = p.globalPowerScale;
        }

        float previewPotency = randomScale * globalScale;

        // === 2️⃣ Dynamic prefix
        float range = data.maxMultiplier - data.minMultiplier;
        string prefix = "";
        if (range > 0.5f)
        {
            float normalized = (randomScale - data.minMultiplier) / range;
            if (normalized < 0.33f) prefix = "Poor ";
            else if (normalized > 0.66f) prefix = "Potent ";
        }

        string displayName = prefix + data.cardName.Split(' ')[^1];

        // === 3️⃣ Determine <X> substitution based on card effect
        float effectiveValue = 0f;
        if (data.effects != null)
        {
            foreach (var eff in data.effects)
            {
                if (eff is DamageEffect dmg)
                    effectiveValue = dmg.baseDamage * previewPotency;
                else if (eff is HealEffect heal)
                    effectiveValue = heal.baseHeal * previewPotency;
                else if (eff is BlockEffect blk)
                    effectiveValue = blk.baseBlock * previewPotency;
            }
        }

        string coloredValue = $"<color=#FFCF40>{effectiveValue:F0}</color>";

        // === 4️⃣ Create display strings (don’t mutate data)
        string desc = data.description;
        string intent = data.intentionText;

        if (!string.IsNullOrEmpty(desc) && desc.Contains("<X>"))
            desc = desc.Replace("<X>", coloredValue);

        if (!string.IsNullOrEmpty(intent) && intent.Contains("<X>"))
            intent = intent.Replace("<X>", coloredValue);

        // === 5️⃣ Render tooltip
        titleText.text = displayName;
        descriptionText.text = desc;
        statsText.text = $"Type: {data.cardType}\nCost: {data.energy}";

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(true);

        Debug.Log($"🟨 Tooltip simulated potency={previewPotency:F2}, {displayName}, effective={effectiveValue:F0}");
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
