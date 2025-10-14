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
    private CardRunner runner;          // 👈 actually declare the field
    public CardRunner Runner => runner; // 👈 public read-only accessor

    private void Awake()
    {
        runner = GetComponent<CardRunner>();
        cardBase = GetComponent<CardBase>();
        if (cardBase == null)
            Debug.LogWarning($"CardView '{name}' has no CardBase component! Add one like AttackCard.");

        if (imageSR == null)
            imageSR = GetComponent<SpriteRenderer>();

        dragHandler = GetComponent<CardDragHandler>();
        handView = FindFirstObjectByType<HandView>();
    }

    public bool UseCard(Collider2D target)
    {
        if (player is Player p)
        {
            if (!p.UseEnergy(runner.EnergyCost))
            {
                Debug.Log("Not enough energy");
                return false;
            }

            if (runner.TryPlay(player, target, out _))
            {
                if (handView) handView.RemoveCard(this);
                transform.DOScale(Vector3.zero, 0.15f).OnComplete(() => gameObject.SetActive(false));
                return true;
            }

            return false;
        }
        return false;
    }

    private void OnMouseEnter()
    {
        if (handView != null && !dragHandler.IsDragging)
            handView.OnHover(this);

        if (TooltipManager.Instance != null && runner != null && runner.data != null)
            TooltipManager.Instance.ShowTooltip(runner.data, runner, player);
    }

    private void OnMouseExit()
    {
        if (handView != null && !dragHandler.IsDragging)
            handView.OnHoverExit(this);

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

    public void Setup(CardBase card)
    {
        if (card == null) return;

        cardBase = card;
        if (imageSR != null)
            imageSR.sprite = card.artwork;

        wrapper?.SetActive(true);
    }
}
