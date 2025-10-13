using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image healthFill;       // HealthBarFill
    [SerializeField] private TMP_Text healthText;    // HealthText
    [SerializeField] private GameObject defensePanel; // DefensePanel (parent)
    [SerializeField] private Image defenseIcon;      // DefenseIcon (optional)
    [SerializeField] private TMP_Text defenseText;   // DefenseText

    private CharacterBase character;
    private Vector3 offset = new Vector3(0, -1.2f, 0);

    // =============================
    // INITIALIZATION
    // =============================

    public void Initialize(CharacterBase target)
    {
        character = target;

        UpdateHealth(target.currentHealth, target.maxHealth);
        UpdateBlock(target.block);
    }

    // =============================
    // HEALTH
    // =============================

    public void UpdateHealth(int current, int max)
    {
        if (healthFill == null) return;

        float fill = Mathf.Clamp01((float)current / max);
        healthFill.rectTransform.localScale = new Vector3(fill, 1f, 1f);

        if (healthText != null)
            healthText.text = $"{current}/{max}";
    }

    // =============================
    // BLOCK (DEFENSE PANEL)
    // =============================

    public void UpdateBlock(int block)
    {
        if (defensePanel == null) return;

        bool hasBlock = block > 0;
        defensePanel.SetActive(hasBlock);

        if (hasBlock && defenseText != null)
            defenseText.text = $"+{block}";

        // Optional: pulse icon when gaining block
        if (hasBlock && defenseIcon != null)
        {
            defenseIcon.color = Color.cyan;
            defenseIcon.CrossFadeColor(Color.white, 0.5f, false, true);
        }
    }

    // =============================
    // LATE UPDATE (POSITION + SYNC)
    // =============================

    private void LateUpdate()
    {
        if (character == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = character.transform.position + offset;

        // Live updates (lightweight enough for now)
        UpdateHealth(character.currentHealth, character.maxHealth);
        UpdateBlock(character.block);
    }
}
