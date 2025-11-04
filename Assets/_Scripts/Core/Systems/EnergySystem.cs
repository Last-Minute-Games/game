using UnityEngine;
using TMPro;
using DG.Tweening;

public class EnergySystem : MonoBehaviour
{
    public static EnergySystem Instance { get; private set; }

    private BattleSystem _battleSystem;

    [Header("Energy Settings")]
    public int maxEnergy = 3;
    public int currentEnergy;

    [Header("UI Reference")]
    public TMP_Text energyText;

    private bool _turnEndRequested = false; // 🔒 prevents duplicate turn-end calls

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _battleSystem = FindFirstObjectByType<BattleSystem>();
        currentEnergy = maxEnergy;
        UpdateUI();
    }

    // -----------------------------------------------------
    // CORE ENERGY CONSUMPTION
    // -----------------------------------------------------
    public bool UseEnergy(int amount)
    {
        if (amount <= 0) return true; // 0-cost cards still allowed

        if (currentEnergy < amount)
        {
            Debug.Log("❌ Not enough energy!");
            return false;
        }

        currentEnergy -= amount;
        AnimateUI();
        UpdateUI();

        if (currentEnergy <= 0)
        {
            TryRequestTurnEnd();
        }

        return true;
    }

    // -----------------------------------------------------
    // SAFE TURN END REQUEST
    // -----------------------------------------------------
    private void TryRequestTurnEnd()
    {
        if (_turnEndRequested) return;           // already requested
        _turnEndRequested = true;

        if (_battleSystem != null)
        {
            Debug.Log("🔋 Energy depleted — requesting safe turn end.");
            _battleSystem.RequestTurnEnd("Energy");
        }
        else
        {
            Debug.LogWarning("⚠️ No BattleSystem found for energy depletion end-turn.");
        }
    }

    // Reset flag each time new turn begins
    public void OnNewTurn()
    {
        _turnEndRequested = false;
    }

    // -----------------------------------------------------
    // REFILL / UI
    // -----------------------------------------------------
    public void RefillEnergy()
    {
        currentEnergy = maxEnergy;
        _turnEndRequested = false; // reset protection
        AnimateUI();
        UpdateUI();
    }

    private void AnimateUI()
    {
        if (energyText == null) return;

        energyText.transform.DOKill();
        energyText.transform.DOScale(1.25f, 0.1f)
            .OnComplete(() => energyText.transform.DOScale(1f, 0.15f));
    }

    public void UpdateUI()
    {
        if (energyText)
            energyText.text = $"{currentEnergy}/{maxEnergy}";
    }
}
