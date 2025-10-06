using UnityEngine;
using TMPro;

public class EnergySystem : MonoBehaviour
{
  // actual energy settings
  public int maxEnergy = 3;
  public int currentEnergy;

  public TMP_Text energyText; // UI reference

  void Start()
  {
    currentEnergy = maxEnergy;
    UpdateUI();
  }

  public bool UseEnergy(int amount)
  {
    if (currentEnergy >= amount)
    {
      currentEnergy -= amount;
      UpdateUI();
      Debug.Log($"Used {amount} energy. Remaining: {currentEnergy}");
      return true;
    }
    else
    {
      Debug.Log("Not enough energy!");
      return false;
    }
  }
  public void RefillEnergy()
  {
    currentEnergy = maxEnergy;
    UpdateUI();
    Debug.Log($"Energy refilled! Current: {currentEnergy}");
  }

  private void UpdateUI()
  {
    if (energyText != null)
    {
      energyText.text = $"{currentEnergy}/{maxEnergy}";
    }
  }
  void Update()
  {
      if (Input.GetKeyDown(KeyCode.Space))
          UseEnergy(1);

      if (Input.GetKeyDown(KeyCode.R))
          RefillEnergy();
  }
}


