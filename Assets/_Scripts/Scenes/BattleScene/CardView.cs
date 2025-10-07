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


    private void OnMouseEnter()
    {
        // Find hand and tell it we are hovered
        HandView hand = FindObjectOfType<HandView>();
        if (hand != null)
            hand.OnHover(this);

        // Show tooltip
        if (TooltipManager.Instance != null && cardBase != null)
            TooltipManager.Instance.ShowTooltip(cardBase);
    }

    private void OnMouseExit()
    {
        // Reset hover on hand
        HandView hand = FindObjectOfType<HandView>();
        if (hand != null)
            hand.OnHoverExit(this);

        // Hide tooltip
        if (TooltipManager.Instance != null)
            TooltipManager.Instance.HideTooltip();
    }

    public bool TryUseCardOn(Enemy enemy, Player player)
    {
        if (cardBase == null || player == null) return false;

        if (cardBase.cardType == CardType.Attack && enemy != null)
        {
            cardBase.Use(player, enemy);
            AnimateCardPlay(enemy.transform);
            return true;
        }
        else if ((cardBase.cardType == CardType.Healing || cardBase.cardType == CardType.Defense) && player != null)
        {
            cardBase.Use(player, player);
            AnimateCardPlay(player.transform);
            return true;
        }

        return false;
    }

    private void AnimateCardPlay(Transform target)
    {
        transform.DOMove(target.position, 0.2f)
            .OnComplete(() =>
            {
                transform.DOScale(Vector3.zero, 0.1f)
                    .OnComplete(() => gameObject.SetActive(false));
            });
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
