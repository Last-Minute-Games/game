using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class CardView : MonoBehaviour
{
  [SerializeField] private TMP_Text title;

  [SerializeField] private TMP_Text description;

  [SerializeField] private TMP_Text mana;

  [SerializeField] private SpriteRenderer iconSR;

  [SerializeField] private SpriteRenderer iconBackground;

  [SerializeField] private SortingGroup sortingGroup;

  [SerializeField] private GameObject wrapper;

  public void SetSortingOrder(int order) // expose order to manage sorting group for cards
  {
    sortingGroup.sortingOrder = order;
  }
}
