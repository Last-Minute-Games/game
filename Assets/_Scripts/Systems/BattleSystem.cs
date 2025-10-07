using UnityEngine;

public class TestSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private HandView handView;
    [SerializeField] private CardData cardData;
    
    [SerializeField] private CardView cardViewPrefab;

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space)) {
        //     
        // }
    }

    private void Start()
    {
        for (var i = 0; i < 7; i++) {
            Card card = new(cardData);
            CardView cardView = CardViewCreator.Instance.CreateCardView(card, transform.position, Quaternion.identity);
            StartCoroutine(handView.AddCard(cardView));
        }
    }
}
