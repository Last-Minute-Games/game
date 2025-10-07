using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;
    public TMP_Text healthText;

    private CharacterBase character;
    private Vector3 offset = new Vector3(0, 1.5f, 0);

    public void Initialize(CharacterBase target)
    {
        character = target;
        UpdateBar();
    }

    public void UpdateBar()
    {
        if (character == null || fillImage == null) return;

        float fill = (float)character.currentHealth / character.maxHealth;
        fillImage.rectTransform.localScale = new Vector3(fill, 1f, 1f);

        if (healthText != null)
            healthText.text = $"{character.currentHealth}/{character.maxHealth}";
    }

    void LateUpdate()
    {
        if (character != null)
        {
            transform.position = character.transform.position + offset;
            UpdateBar();
        }
    }
}
