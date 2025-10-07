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

    [Header("Hover Animation")]
    public float hoverLift = 0.2f;          // smaller lift height
    public float hoverScale = 1.08f;        // subtle scaling
    public float hoverDuration = 0.12f;     // quick but smooth
    public Color hoverColor = Color.yellow; // tint when hovered

    private Vector3 basePosition;
    private Vector3 baseScale;
    private Color baseColor;
    private bool isHovered = false;

    private void Awake()
    {
        cardBase = GetComponent<CardBase>();
        if (cardBase == null)
            Debug.LogWarning($"CardView '{name}' has no CardBase component! Add one like AttackCard.");

        if (imageSR == null)
            imageSR = GetComponent<SpriteRenderer>();

        if (imageSR != null)
            baseColor = imageSR.color;

        baseScale = transform.localScale;
    }

    public void Setup(CardBase card)
    {
        cardBase = card;
        if (imageSR != null && card != null)
            imageSR.sprite = card.artwork;

        wrapper?.SetActive(true);
    }


    private void OnMouseEnter()
    {
        FindObjectOfType<HandView>().OnHover(this);
    }

    private void OnMouseExit()
    {
        FindObjectOfType<HandView>().OnHoverExit(this);
    }

    private void OnMouseDown()
    {
        if (cardBase == null || player == null)
            return;

        if (player is Player p && p.UseEnergy(cardBase.energy))
        {
            cardBase.Use(player, targetEnemy);

            // Find HandView and remove this card
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
}
