using UnityEngine;
using DG.Tweening;

public class OverhaulCardViewHover : Singleton<OverhaulCardViewHover>
{
  [SerializeField] private OverhaulCardView cardViewHover;

  public void Show(OverhaulCard card, Quaternion initRotation, Vector3 initPos, Vector3 finalPos, float duration)
  {
    cardViewHover.Setup(card); // populate w/data

    // pre-set the card to the side 
    cardViewHover.transform.position = initPos;
    cardViewHover.transform.rotation = initRotation;

    cardViewHover.gameObject.SetActive(true); // make visible

    // rotation animation
    cardViewHover.transform.DOMove(finalPos, 0.25f);
    cardViewHover.transform.DORotateQuaternion(Quaternion.identity, 0.25f); //
  }

  public void Hide()
  {
    cardViewHover.gameObject.SetActive(false); // hide hover card
  }


  // cards[i].transform.DOMove(splinePosition + transform.position + 0.01f * i * Vector3.back, duration); // (final destination, float duration)
  // cards[i].transform.DORotate(rotation.eulerAngles, duration);
}
