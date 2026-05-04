using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class OverhaulCardSystem : Singleton<OverhaulCardSystem>
{
  [SerializeField] private OverhaulHandView handView;

  [SerializeField] private Transform drawPilePoint;

  [SerializeField] private Transform discardPilePoint;

  private readonly List<OverhaulCard> drawPile = new();

  private readonly List<OverhaulCard> discardPile = new();

  private readonly List<OverhaulCard> hand = new();

  void OnEnable()
  {
    OverhaulActionSystem.AttachPerformer<OverhaulDrawCardsGA>(DrawCardsPerformer);
    OverhaulActionSystem.AttachPerformer<OverhaulDiscardAllCardsGA>(DiscardAllCardsPerformer);

    OverhaulActionSystem.SubscribeReaction<OverhaulEnemyTurnGA>(EnemyTurnPreReaction, OverhaulReactionTiming.PRE);
    OverhaulActionSystem.SubscribeReaction<OverhaulEnemyTurnGA>(EnemyTurnPostReaction, OverhaulReactionTiming.POST);
  }

  void OnDisable()
  {
    OverhaulActionSystem.DetachPerformer<OverhaulDrawCardsGA>();
    OverhaulActionSystem.DetachPerformer<OverhaulDiscardAllCardsGA>();

    OverhaulActionSystem.UnsubscribeReaction<OverhaulEnemyTurnGA>(EnemyTurnPreReaction, OverhaulReactionTiming.PRE);
    OverhaulActionSystem.UnsubscribeReaction<OverhaulEnemyTurnGA>(EnemyTurnPostReaction, OverhaulReactionTiming.POST);
  }

  // Publics

  public void Setup(List<OverhaulCardData> deckData)
  {
    foreach (var cardData in deckData)
    {
      OverhaulCard card = new(cardData);
      drawPile.Add(card);
    }
  }

  // Performers

  private IEnumerator DrawCardsPerformer(OverhaulDrawCardsGA drawCardsGA)
  {
    int actualAmount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);
    int notDrawnAmount = drawCardsGA.Amount - actualAmount;

    for (int i = 0; i < actualAmount; i++)
    {
      yield return DrawCard();
    }

    if (notDrawnAmount > 0)
    {
      RefillDeck();
      for (int i = 0; i < notDrawnAmount; i++)
      {
        yield return DrawCard();
      }
    }
  }

  private IEnumerator DiscardAllCardsPerformer(OverhaulDiscardAllCardsGA discardAllCardsGA)
  {
    foreach (var card in hand)
    {
      discardPile.Add(card);
      OverhaulCardView cardView = handView.RemoveCard(card);
      yield return DiscardCard(cardView);
    }
    hand.Clear();
  }

  // Reactions

  private void EnemyTurnPreReaction(OverhaulEnemyTurnGA enemyTurnGA)
  {
    OverhaulDiscardAllCardsGA discardAllCardsGA = new();
    OverhaulActionSystem.Instance.AddReaction(discardAllCardsGA);
  }

  private void EnemyTurnPostReaction(OverhaulEnemyTurnGA enemyTurnGA)
  {
    OverhaulDrawCardsGA drawCardsGA = new(5);
    OverhaulActionSystem.Instance.AddReaction(drawCardsGA);
  }


  // Helpers

  private IEnumerator DrawCard()
  {
    OverhaulCard card = drawPile.Draw();
    hand.Add(card);
    OverhaulCardView cardView = OverhaulCardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
    yield return handView.AddCard(cardView);

  }

  private void RefillDeck()
  {
    drawPile.AddRange(discardPile);
    discardPile.Clear();
  }

  private IEnumerator DiscardCard(OverhaulCardView cardView)
  {
    cardView.transform.DOScale(Vector3.zero, 0.15f);
    Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);
    yield return tween.WaitForCompletion();
  }
}
