using UnityEngine;

public class TestSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private HandView handView; 
   

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            CardView cardView = CardViewCreator.Instance.CreateCardView(transform.position, Quaternion.identity);
            StartCoroutine(handView.AddCard(cardView));
        }
    }

}
