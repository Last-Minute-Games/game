using UnityEngine;

public class TestSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private HandView handView;
    [SerializeField] private CardData cardData;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Card card = new(cardData);
            CardView cardView = CardViewCreator.Instance.CreateCardView(card,transform.position, Quaternion.identity);
            StartCoroutine(handView.AddCard(cardView));
        }
    }

}
