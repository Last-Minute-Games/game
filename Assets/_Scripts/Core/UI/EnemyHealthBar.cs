using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : HealthBarBase
{
    [Header("References")]
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject defensePanel;
    [SerializeField] private Image defenseIcon;
    [SerializeField] private TMP_Text defenseText;

    private CharacterBase character;
    private readonly Vector3 offset = new(0, -1.2f, 0);

    public override void Initialize(CharacterBase target)
    {
        character = target;
        UpdateHealth(target.currentHealth, target.maxHealth);
        UpdateBlock(target.block);
    }

    public override void UpdateHealth(int current, int max)
    {
        if (!healthFill) return;
        float fill = Mathf.Clamp01((float)current / max);
        healthFill.rectTransform.localScale = new Vector3(fill, 1f, 1f);
        if (healthText) healthText.text = $"{current}/{max}";
    }

    public override void UpdateBlock(int block)
    {
        if (!defensePanel) return;
        bool hasBlock = block > 0;
        defensePanel.SetActive(hasBlock);
        if (hasBlock && defenseText) defenseText.text = $"+{block}";
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
        if (!character)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = character.transform.position + offset;
        UpdateHealth(character.currentHealth, character.maxHealth);
        UpdateBlock(character.block);
    }
}
