using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System.Collections.Generic;

public class HandView : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;

    private readonly List<CardView> cards = new();

    public IEnumerator AddCard(CardView cardView)
    {
        cards.Add(cardView);
        yield return UpdateCardPositions(0.15f);
    }
    private IEnumerator UpdateCardPositions(float duration) { 
        if(cards.Count == 0) yield break;
        float cardSpacing = 1f / 10f;
        float firstCardPostion = 0.5f - (cards.Count - 1) * cardSpacing / 2;

        Spline spline = splineContainer.Spline;
        for (int i = 0; i < cards.Count; i++) {
            float p = firstCardPostion + i * cardSpacing;
            Vector3 splinePostion = spline.EvaluatePosition(p);
            Vector3 forward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up,forward).normalized);
            cards[i].transform.DOMove(splinePostion + transform.position + 0.01f * i * Vector3.back, duration);
            cards[i].transform.DORotate(rotation.eulerAngles, duration);
        }

        yield return new WaitForSeconds(duration);
    }
   
}
