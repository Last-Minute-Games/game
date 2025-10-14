using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBar : HealthBarBase
{
    [Header("HUD References")]
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject defensePanel;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private Image defenseIcon;

    private CharacterBase player;

    public override void Initialize(CharacterBase target)
    {
        player = target;
        UpdateHealth(target.currentHealth, target.maxHealth);
        UpdateBlock(target.block);
    }

    public override void UpdateHealth(int current, int max)
    {
        if (!healthFill) return;
        float ratio = Mathf.Clamp01((float)current / max);
        healthFill.fillAmount = ratio;

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

        if (healthFill)
            healthFill.color = hasBlock ? new Color(0.5f, 0.8f, 1f) : Color.red;
    }
}
