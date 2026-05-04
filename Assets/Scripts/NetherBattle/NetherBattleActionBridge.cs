using System.Collections;
using DG.Tweening;
using Entities.Enemies.Manager;
using GameItems;
using GameItems.Cards;
using GameItems.Cards.Helpers;
using UnityEngine;

/// <summary>
/// Hooks production PlayerManager / DeckViewer into <see cref="OverhaulActionSystem"/> with sequential draw/discard (OverhaulNetherScripts parity).
/// Add <see cref="OverhaulActionSystem"/> to the scene and assign references (or rely on auto-resolve from <see cref="RoundManager"/>).
/// </summary>
public class NetherBattleActionBridge : MonoBehaviour
{
  [SerializeField] private RoundManager roundManager;

  [SerializeField] private PlayerManager player;

  [SerializeField] private EnemyManager enemyManager;

  [SerializeField] private DeckViewer handViewer;

  private void Awake()
  {
    if (roundManager == null)
      roundManager = FindFirstObjectByType<RoundManager>();
    if (player == null && roundManager != null)
      player = roundManager.player;
    if (enemyManager == null && roundManager != null)
      enemyManager = roundManager.enemyManager;
    if (handViewer == null && roundManager != null)
      handViewer = roundManager.handViewer;
  }

  private void OnEnable()
  {
    OverhaulActionSystem.AttachPerformer<NetherDrawCardsGA>(DrawCardsPerformer);
    OverhaulActionSystem.AttachPerformer<NetherDiscardAllHandGA>(DiscardAllHandPerformer);
    OverhaulActionSystem.AttachPerformer<NetherEnemyPhaseGA>(EnemyPhasePerformer);
  }

  private void OnDisable()
  {
    OverhaulActionSystem.DetachPerformer<NetherDrawCardsGA>();
    OverhaulActionSystem.DetachPerformer<NetherDiscardAllHandGA>();
    OverhaulActionSystem.DetachPerformer<NetherEnemyPhaseGA>();
  }

  private IEnumerator DrawCardsPerformer(NetherDrawCardsGA ga)
  {
    if (player == null)
      yield break;

    var cm = player.cardManager;
    int amount = Mathf.Max(0, ga.Amount);

    for (int i = 0; i < amount; i++)
    {
      int handBefore = cm.hand.Count;
      cm.DrawCard();
      if (cm.hand.Count == handBefore)
        break;

      if (handViewer != null)
      {
        handViewer.SetPlayer(player);
        handViewer.SetSource(DeckViewer.Source.Hand, rebuild: false);
        handViewer.RebuildSmart();
      }

      float wait = handViewer != null ? handViewer.LayoutTweenDuration : 0.15f;
      yield return new WaitForSeconds(wait);

      roundManager?.RefreshPileViewersOnly();
    }
  }

  private IEnumerator DiscardAllHandPerformer(NetherDiscardAllHandGA ga)
  {
    if (player == null)
      yield break;

    var cm = player.cardManager;
    Vector3 target = handViewer != null
      ? handViewer.GetDiscardTargetWorldPosition()
      : (roundManager != null && roundManager.handViewer != null
        ? roundManager.handViewer.GetDiscardTargetWorldPosition()
        : Vector3.zero);

    while (cm.hand.Count > 0)
    {
      CardInstance inst = cm.handInstances.Count > 0 ? cm.handInstances[0] : null;
      CardData data = cm.hand.Count > 0 ? cm.hand[0] : null;

      CardRender render = handViewer != null
        ? handViewer.FindRenderForHandCard(inst, data)
        : null;

      if (render != null)
      {
        DeckViewer.PrepareCardVisualForDiscard(render);
        render.transform.SetParent(null, true);

        Sequence seq = DOTween.Sequence();
        seq.Append(render.transform.DOMove(target, 0.15f));
        seq.Join(render.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InQuad));
        yield return seq.WaitForCompletion();

        if (handViewer != null)
          handViewer.UnregisterRender(render);
        Destroy(render.gameObject);
      }

      cm.DiscardHandCardAtIndex(0);
    }
  }

  private IEnumerator EnemyPhasePerformer(NetherEnemyPhaseGA ga)
  {
    if (enemyManager == null || player == null || player.playerData == null)
      yield break;

    yield return new WaitForSeconds(0.5f);

    yield return enemyManager.ExecuteEnemyTurnSequence(player.playerData);

    yield return new WaitForSeconds(0.5f);

    if (enemyManager != null)
      enemyManager.ResetAllEnemyBlock();
  }
}
