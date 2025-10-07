using UnityEngine;
using TMPro;
using UnityEngine.Rendering;

public class CardView : MonoBehaviour
{
    //[SerializeField] private TMP_Text title;

    //[SerializeField] private TMP_Text description;

    //[SerializeField] private TMP_Text mana;

    [SerializeField] public SpriteRenderer imageSR;
    
    [field: SerializeField] public int FixedManaCost { get; private set; } = 1;

    [SerializeField] private GameObject wrapper;
    
    [SerializeField] public SortingGroup sortingGroup;

    public Card Card { get; private set; }
    public void Setup(Card card) {
        Card = card;
        //title.text = card.Title;
        //description.text = card.Description;
        //mana.text = card.Mana.ToString();
        imageSR.sprite = card.Image;
        wrapper.SetActive(true);
    }
    
    // on click
    void OnMouseDown()
    {
        // if (Card == null) return;
        Debug.Log("CardView: OnMouseDown: " + Card);
        
    }
    
    void OnMouseEnter()
    {
        wrapper.SetActive(false);
        Vector3 pos = new(transform.position.x, -2,0);
        CardViewHoverSystem.Instance.Show(Card, pos);
    }
    
    
    void OnMouseExit()
    {
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);

    }
}
