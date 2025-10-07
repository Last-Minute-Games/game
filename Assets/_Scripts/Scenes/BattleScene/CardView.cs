using _Scripts.Gameplay;
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
    private CardDragHandler dragHandler;
    private HandView handView;
    
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
        
        dragHandler = GetComponent<CardDragHandler>();
        handView = FindFirstObjectByType<HandView>();
    }
    
    // public bool ActivateCard(CharacterBase target)
    // {
    //     if (cardBase == null || player == null) return false;
    //
    //     if (cardBase.cardType == CardType.Attack && target != null && target is Enemy enemy)
    //     {
    //         cardBase.Use(player, target);
    //         AnimateCardPlay(enemy.transform);
    //         return true;
    //     }
    //     else if ((cardBase.cardType == CardType.Healing || cardBase.cardType == CardType.Defense) && player != null)
    //     {
    //         cardBase.Use(player, target);
    //         AnimateCardPlay(player.transform);
    //         return true;
    //     }
    //
    //     return false;
    // }

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
            try {
                cardBase.Use(player, targetComponent);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error using card {cardBase.cardName}: {ex.Message}");
            }
            
            Debug.Log(handView);
            
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
        if (handView != null && !dragHandler.IsDragging)
            handView.OnHover(this);

        // Show tooltip
        if (TooltipManager.Instance != null && cardBase != null)
            TooltipManager.Instance.ShowTooltip(cardBase);
    }

    private void OnMouseExit()
    {
        // Reset hover on hand
        if (handView != null && !dragHandler.IsDragging)
            handView.OnHoverExit(this);

        // Hide tooltip
        if (TooltipManager.Instance != null)
            TooltipManager.Instance.HideTooltip();
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
