using UnityEngine;

public class OverhaulEndTurnButtonUI : MonoBehaviour
{
  public void OnClick()
  {
    OverhaulEnemyTurnGA enemyTurnGA = new();
    OverhaulActionSystem.Instance.Perform(enemyTurnGA);
  }
}
