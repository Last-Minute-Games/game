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
}
