using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerPrefab : MonoBehaviour
{
    [Header("References")]
    public PlayerData playerData;

    [Header("Energy UI")]
    public TextMeshProUGUI energyText;

    [Header("Healthbar UI")]
    public Image healthbarFill;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI shieldText;

    private void Start()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerData not assigned to PlayerPrefab!");
            return;
        }

        // Initialize visuals
        SetupUI();
    }

    private void SetupUI()
    {
        // Set Energy
        energyText.text = $"{playerData.baseEnergy}/{playerData.maxEnergy}";

        // Set Healthbar
        healthbarFill.fillAmount = 1f; // full
        healthbarFill.color = new Color32(108, 15, 15, 255);

        healthText.text = $"{playerData.baseHealth}/{playerData.baseHealth}";
        shieldText.text = ""; // empty / invisible
    }
}
