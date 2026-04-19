using UnityEngine;

public class OverhaulTestSystem : MonoBehaviour
{
  [SerializeField] private OverhaulHandView handView;

  [SerializeField] private OverhaulCardData cardData;

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space))
    {
      OverhaulCard card = new(cardData);
      // from singleton cardviewcreator instance, createcardview with position and rotation
      OverhaulCardView cardView = OverhaulCardViewCreator.Instance.CreateCardView(card, transform.position, Quaternion.identity);
      // since its IEnumerator, call w/coroutine
      StartCoroutine(handView.AddCard(cardView));
    }
  }
}
