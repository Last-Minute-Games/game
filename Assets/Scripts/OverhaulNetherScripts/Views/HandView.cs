using System.Collections.Generic;
using System.Collections;
using UnityEngine.Splines;
using UnityEngine;
using DG.Tweening;

public class HandView : MonoBehaviour
{
  [SerializeField] private SplineContainer splineContainer;

  [SerializeField] private readonly float maxHandSize = 10f;

  private readonly List<CardView> cards = new();

  public IEnumerator AddCard(CardView cardView)
  {
    cards.Add(cardView);
    yield return UpdateCardPositions(0.15f);
  }

  private IEnumerator UpdateCardPositions(float duration)
  {
    if (cards.Count == 0) yield break;

    float cardSpacing = 1f / maxHandSize; // calculate spacing based on max hand size
    float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2; // 0.5f is half the size
    Spline spline = splineContainer.Spline;

    // go through each card, position at right position
    for (int i = 0; i < cards.Count; i++)
    {
      // calc float pos p to world space position
      float p = firstCardPosition + i * cardSpacing;
      Vector3 splinePosition = spline.EvaluatePosition(p);

      // calc rotation
      Vector3 forward = spline.EvaluateTangent(p);
      Vector3 up = spline.EvaluateUpVector(p);
      Quaternion rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);

      cards[i].SetSortingOrder(i); // explicitly set the sorting order

      cards[i].transform.DOMove(splinePosition + transform.position + 0.01f * i * Vector3.back, duration); // (final destination, float duration)
      cards[i].transform.DORotate(rotation.eulerAngles, duration);
    }
    yield return new WaitForSeconds(duration);
  }
}
