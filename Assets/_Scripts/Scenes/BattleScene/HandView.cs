using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class HandView : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;

    private readonly List<CardView> _cards = new();

    public IEnumerator AddCard(CardView cardView)
    {
        _cards.Add(cardView);
        yield return UpdateCardPositions(0.15f);
    }
    private IEnumerator UpdateCardPositions(float duration)
    {
        if (_cards.Count == 0) yield break;
        
        float cardSpacing = 1f / 10f;
        float firstCardPos = 0.5f - (_cards.Count - 1) * cardSpacing / 2;

        Spline spline = splineContainer.Spline;
        for (int i = 0; i < _cards.Count; i++)
        {
            float p = firstCardPos + i * cardSpacing;
            
            Vector3 splinePos = spline.EvaluatePosition(p);
            Vector3 forward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            
            Quaternion rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            
            _cards[i].transform.DOMove(splinePos + transform.position + 0.01f * i * Vector3.back, duration);
            _cards[i].transform.DORotate(rotation.eulerAngles, duration);
            
            _cards[i].sortingGroup.sortingOrder = i;    
        }

        yield return new WaitForSeconds(duration);
    }

}
