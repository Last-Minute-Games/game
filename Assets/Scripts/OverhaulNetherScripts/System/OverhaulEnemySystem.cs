using System.Collections;
using UnityEngine;

public class OverhaulEnemySystem : MonoBehaviour
{
  void OnEnable()
  {
    OverhaulActionSystem.AttachPerformer<OverhaulEnemyTurnGA>(EnemyTurnPerformer);
  }
  void OnDisable()
  {
    OverhaulActionSystem.DetachPerformer<OverhaulEnemyTurnGA>();
  }

  // Performers

  private IEnumerator EnemyTurnPerformer(OverhaulEnemyTurnGA enemyTurnGA)
  {
    Debug.Log("enemy turn :D");
    yield return new WaitForSeconds(2f);
    Debug.Log("enemy turn ended");
  }
}
