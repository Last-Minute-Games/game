using UnityEngine;
using DG.Tweening;
using UnityEngine.XR;

[RequireComponent(typeof(SpriteRenderer))]
public class CardView : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer imageSR;
    [SerializeField] private GameObject wrapper;
    [SerializeField] public UnityEngine.Rendering.SortingGroup sortingGroup;

    private BoxCollider2D cardCollider;
    
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

    public void UseCard(Collider2D target)
    {
        if (cardBase == null || player == null) return;

        var targetComponent = target?.GetComponent<CharacterBase>();
        
        if (target != null)
        {
            // check if target is enemy or player themselves
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
                targetComponent = enemy;
            else if (target.GetComponent<Player>() != null)
                targetComponent = player; // allow healing/defense on self
        }
        
        if (targetComponent == null)
        {
            Debug.Log("Invalid target for this card.");
            return;
        }
        
        if (player is Player p && p.UseEnergy(cardBase.energy))
        {
            cardBase.Use(player, targetComponent);
            AnimateCardPlay(target.transform);

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

    private void OnMouseDown()
    {
        // UseCard();
    }

    public bool TryUseCardOn(Enemy enemy)
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
        Debug.Log("Animating card to " + target.name);
        transform.DOMove(target.position, 0.2f)
            .OnComplete(() =>
            {
                transform.DOScale(Vector3.zero, 0.1f)
                    .OnComplete(() => transform.gameObject.SetActive(false));
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
