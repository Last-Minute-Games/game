using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public HandController hand;      // set by HandController
    [HideInInspector] public RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hand?.OnCardPointerEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hand?.OnCardPointerExit(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
      hand?.OnCardClicked(this);
    }
}
