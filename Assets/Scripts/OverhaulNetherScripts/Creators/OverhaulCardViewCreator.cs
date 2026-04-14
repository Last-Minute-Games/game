using UnityEngine;
using DG.Tweening;

public class OverhaulCardViewCreator : Singleton<OverhaulCardViewCreator>
{
  [SerializeField] private OverhaulCardView cardViewPrefab;

  public OverhaulCardView CreateCardView(OverhaulCard card, Vector3 position, Quaternion rotation)
  {
    OverhaulCardView cardView = Instantiate(cardViewPrefab, position, rotation); // instantiating cardViewPrefab
    cardView.transform.localScale = Vector3.zero;
    cardView.transform.DOScale(Vector3.one, 0.15f);

    cardView.Setup(card);

    return cardView;
  }
}
