using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] public SpriteRenderer imageSR;
    [SerializeField] private GameObject wrapper;
    [SerializeField] public UnityEngine.Rendering.SortingGroup sortingGroup;

    public Card Card { get; private set; }

    public void Setup(Card card)
    {
        Card = card;
        imageSR.sprite = card.Image;
        wrapper.SetActive(true);
    }

    // Hover in → show enlarged view
    void OnMouseEnter()
    {
        wrapper.SetActive(false);
        Vector3 pos = new(transform.position.x, -2, 0);
        CardViewHoverSystem.Instance.Show(Card, pos);
    }

    // Hover out → restore normal view
    void OnMouseExit()
    {
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }
}
