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

    private void Awake()
    {
        _battleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentEnergy = maxEnergy;
        UpdateUI();
    }

    public bool UseEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            AnimateUI();
            UpdateUI();
            
            if (currentEnergy == 0)
            {
                _battleSystem.EndPlayerTurn();
            }
            
            return true;
        }

        Debug.Log("Not enough energy!");
        return false;
    }

    public void RefillEnergy()
    {
        currentEnergy = maxEnergy;
        AnimateUI();
        UpdateUI();
    }

    private void AnimateUI()
    {
        if (energyText == null) return;

        // Pulse animation (like Slay the Spire)
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
