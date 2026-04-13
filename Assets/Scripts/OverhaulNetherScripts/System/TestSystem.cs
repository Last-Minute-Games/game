using UnityEngine;

public class TestSystem : MonoBehaviour
{
  [SerializeField] private HandView handView;

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space))
    {
      // from singleton cardviewcreator instance, createcardview with position and rotation
      CardView cardView = CardViewCreator.Instance.CreateCardView(transform.position, Quaternion.identity);
      // since its IEnumerator, call w/coroutine
      StartCoroutine(handView.AddCard(cardView));
    }
  }
}
