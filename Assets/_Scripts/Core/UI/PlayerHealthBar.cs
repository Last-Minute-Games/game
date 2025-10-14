using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PlayerHealthBar : HealthBarBase
{
    [Header("HUD References")]
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject defensePanel;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private Image defenseIcon;

    [Header("Feedback")]
    [SerializeField] private GameObject floatingTextPrefab; // small TMP text prefab
    [SerializeField] private Transform floatingTextAnchor;   // anchor above health bar

    private CharacterBase player;
    private Color originalHealthColor;

    public override void Initialize(CharacterBase target)
    {
        player = target;
        if (healthFill != null)
            originalHealthColor = healthFill.color;

        UpdateHealth(target.currentHealth, target.maxHealth);
        UpdateBlock(target.block);
    }

    public override void UpdateHealth(int current, int max)
    {
        if (!healthFill) return;

        float fill = Mathf.Clamp01((float)current / max);
        healthFill.rectTransform.DOScaleX(fill, 0.25f).SetEase(Ease.OutCubic);

        if (healthText)
            healthText.text = $"{current}/{max}";
    }

    public override void UpdateBlock(int block)
    {
        if (!defensePanel) return;

        bool hasBlock = block > 0;
        defensePanel.SetActive(hasBlock);

        if (hasBlock && defenseText)
            defenseText.text = $"+{block}";

        if (hasBlock && defenseIcon)
        {
            defenseIcon.color = Color.cyan;
            defenseIcon.CrossFadeColor(Color.white, 0.5f, false, true);
        }

        if (healthFill)
            healthFill.color = hasBlock ? new Color(0.5f, 0.8f, 1f) : originalHealthColor;
    }

    private void LateUpdate()
    {
        if (!player)
        {
            Destroy(gameObject);
            return;
        }

        UpdateHealth(player.currentHealth, player.maxHealth);
        UpdateBlock(player.block);
    }

    // ================================================
    // 🔹 FEEDBACK HELPERS
    // ================================================
    public void ShowFloatingText(string message, Color color)
    {
        if (floatingTextPrefab == null) return;

        GameObject textObj = Instantiate(floatingTextPrefab, floatingTextAnchor ?? transform);
        TMP_Text tmp = textObj.GetComponent<TMP_Text>();
        tmp.text = message;
        tmp.color = color;

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        // float slightly downward, since enemy text floats upward
        rt.DOAnchorPosY(rt.anchoredPosition.y - 60f, 1f).SetEase(Ease.OutCubic);
        tmp.DOFade(0f, 1f).SetEase(Ease.InQuad).OnComplete(() => Destroy(textObj));
    }

    public void FlashDamage()
    {
        if (healthFill == null) return;

        healthFill.DOColor(Color.red, 0.1f)
            .OnComplete(() => healthFill.DOColor(originalHealthColor, 0.3f));
    }

    public void FlashHeal()
    {
        if (healthFill == null) return;

        healthFill.DOColor(Color.green, 0.15f)
            .OnComplete(() => healthFill.DOColor(originalHealthColor, 0.4f));
    }

    public void FlashBlock()
    {
        if (healthFill == null) return;

        healthFill.DOColor(new Color(0.5f, 0.8f, 1f), 0.1f)
            .OnComplete(() => healthFill.DOColor(originalHealthColor, 0.4f));
    }
}
