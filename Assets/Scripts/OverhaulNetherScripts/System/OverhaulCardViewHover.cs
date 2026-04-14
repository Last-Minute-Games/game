using UnityEngine;

public class OverhaulCardViewHover : Singleton<OverhaulCardViewHover>
{
  [SerializeField] private OverhaulCardView cardViewHover;

  public void Show(OverhaulCard card, Vector3 position)
  {
    cardViewHover.gameObject.SetActive(true); // make visiblr
    cardViewHover.Setup(card); // populate
    cardViewHover.transform.position = position; // set relative to that card pos
  }

  public void Hide()
  {
    cardViewHover.gameObject.SetActive(false); // hide
  }
}
