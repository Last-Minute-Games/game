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

        // ✅ Same logic as EnemyHealthBar
        float fill = Mathf.Clamp01((float)current / max);
        healthFill.rectTransform.localScale = new Vector3(fill, 1f, 1f);

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
            healthFill.color = hasBlock ? new Color(0.5f, 0.8f, 1f) : Color.red;
    }

    private void LateUpdate()
    {
        if (!player)
        {
            Destroy(gameObject);
            return;
        }

        // ✅ Match EnemyHealthBar behavior exactly
        UpdateHealth(player.currentHealth, player.maxHealth);
        UpdateBlock(player.block);
    }
}
