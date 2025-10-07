using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class CardView : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer imageSR;
    [SerializeField] private GameObject wrapper;
    [SerializeField] public UnityEngine.Rendering.SortingGroup sortingGroup;

    [Header("Gameplay")]
    public CharacterBase player;
    public CharacterBase targetEnemy;
    private CardBase cardBase;

    private void Awake()
    {
        cardBase = GetComponent<CardBase>();
        if (cardBase == null)
            Debug.LogWarning($"CardView '{name}' has no CardBase component! Add one like AttackCard.");

        if (imageSR == null)
            imageSR = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        if (cardBase == null || player == null) return;

        if (player is Player p && p.UseEnergy(cardBase.energy))
        {
            cardBase.Use(player, targetEnemy);

            HandView handView = FindObjectOfType<HandView>();
            if (handView != null)
                handView.RemoveCard(this);

            transform.DOScale(Vector3.zero, 0.15f).OnComplete(() => gameObject.SetActive(false));
            Debug.Log($"{player.characterName} played {cardBase.cardName}");
        }
        else
        {
            Debug.Log("Not enough energy to play this card!");
        }
    }


    // Restores compatibility with CardViewHoverSystem
    public void Setup(CardBase card)
    {
        if (card == null) return;

        cardBase = card;
        if (imageSR != null)
            imageSR.sprite = card.artwork;

        wrapper?.SetActive(true);
    }
}
