using System.Collections;
using UnityEngine;

public class EnemySystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
    }
    //Performers

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        Debug.Log("Enemy Turn Started");
        yield return new WaitForSeconds(2f);
        Debug.Log("End Enemy Turn");
    }
}
