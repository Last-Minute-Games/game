using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class OverhaulCardView : MonoBehaviour
{
  [SerializeField] private TMP_Text title;

  [SerializeField] private TMP_Text description;

  [SerializeField] private TMP_Text mana;

  [SerializeField] private SpriteRenderer iconSR;

  [SerializeField] private SpriteRenderer iconBackground;

  [SerializeField] private SortingGroup sortingGroup;

  [SerializeField] private GameObject wrapper;

  public OverhaulCard Card { get; private set; }

  // expose order to manage sorting group for cards
  public void SetSortingOrder(int order)
  {
    sortingGroup.sortingOrder = order;
  }

  // initialize the PHYSICAl card in the real world from OverhaulCard
  public void Setup(OverhaulCard card)
  {
    Card = card;
    title.text = card.Title;
    description.text = card.Description;
    mana.text = card.Mana.ToString();
    iconSR.sprite = card.Icon;
    iconBackground.color = card.IconBackgroundColor;
  }

  void OnMouseOver()
  {
    Debug.Log("Mouse is over card " + title);
    wrapper.SetActive(false); // set the surrounding wrapper to false
    Vector3 initPos = new(transform.position.x, transform.position.y, 0);
    Vector3 finalPos = new(transform.position.x, -2, 0);
    Quaternion initRotation = transform.rotation;

    OverhaulCardViewHover.Instance.Show(Card, initRotation, initPos, finalPos, 0.25f);
  }

  void OnMouseExit()
  {
    Debug.Log("Mouse has exited card " + title);
    OverhaulCardViewHover.Instance.Hide();
    wrapper.SetActive(true);
  }
}
