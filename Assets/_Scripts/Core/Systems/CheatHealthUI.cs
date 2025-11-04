using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheatHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField healthInput;
    [SerializeField] private Button applyButton;
    [SerializeField] private BattleSystem battleSystem; // optional, auto-finds if not assigned

    private void OnEnable()
    {
        if (battleSystem == null)
            battleSystem = FindFirstObjectByType<BattleSystem>();

        if (applyButton != null)
        {
            applyButton.onClick.RemoveAllListeners();   // ✅ clears duplicates
            applyButton.onClick.AddListener(OnApplyClicked);
        }
    }

    private void OnApplyClicked()
    {
        if (battleSystem == null)
        {
            Debug.LogWarning("⚠️ CheatHealthUI: No BattleSystem found!");
            return;
        }

        var player = GetPlayer();
        if (player == null)
        {
            Debug.LogWarning("⚠️ CheatHealthUI: Player reference not found in BattleSystem!");
            return;
        }

        if (!int.TryParse(healthInput.text, out int amount))
        {
            Debug.LogWarning("⚠️ Invalid input for health amount.");
            return;
        }

        AddHealth(player, amount);
        Debug.Log($"[CHEAT] Added {amount} HP to Player (new total: {player.currentHealth})");
    }

    private Player GetPlayer()
    {
        // Access the private player field in BattleSystem
        var playerField = typeof(BattleSystem).GetField("player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return playerField?.GetValue(battleSystem) as Player;
    }

    private void AddHealth(Player target, int amount)
    {
        target.currentHealth += amount;
        target.currentHealth = Mathf.Max(0, target.currentHealth); // prevent negative HP
        target.healthBarInstance?.UpdateHealth(target.currentHealth, target.maxHealth);
    }
}
