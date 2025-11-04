using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : HealthBarBase
{
    [Header("References")]
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text defenseText;

    private CharacterBase character;
    private readonly Vector3 offset = new(0, -0.9f, 0);

    public override void Initialize(CharacterBase target)
    {
        Debug.Log(target.currentHealth + "/" + target.maxHealth);
        
        character = target;
        UpdateHealth(target.currentHealth, target.maxHealth);
        UpdateBlock(target.block);
    }

    public override void UpdateHealth(int current, int max)
    {
        
        
        if (!healthFill) return;
        float fill = (float)current / max;
        healthFill.rectTransform.localScale = new Vector3(fill * 23, 30f, 1f);
        if (healthText) healthText.text = $"{current}/{max}";
    }

    public override void UpdateBlock(int block)
    {
        var defaultColor = new Color32(108, 15, 15, 255);
        
        // if (!defensePanel) return;
        bool hasBlock = block > 0;
        // defensePanel.SetActive(hasBlock);
        defenseText.text = $"{block}";
        // if (hasBlock && defenseIcon)
        // {
        //     defenseIcon.color = Color.cyan;
        //     defenseIcon.CrossFadeColor(Color.white, 0.5f, false, true);
        // }
        if (healthFill)
            healthFill.color = hasBlock ? new Color(0.5f, 0.8f, 1f) : defaultColor;
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
