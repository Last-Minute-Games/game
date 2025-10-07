using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPos;
    private CardView cardView;
    private bool isDragging = false;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        cardView = GetComponent<CardView>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = transform.position;
        isDragging = true;

        spriteRenderer.sortingOrder = 500;
        transform.DOScale(1.15f, 0.1f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        transform.position = worldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        transform.DOScale(1f, 0.1f);

        Collider2D target = Physics2D.OverlapPoint(transform.position);
        if (target != null)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            Player player = target.GetComponent<Player>();

            if (cardView != null && cardView.TryUseCardOn(enemy, player))
                return; // card handled properly
        }

        // Snap back if invalid
        transform.DOMove(startPos, 0.2f).SetEase(Ease.OutCubic);
        spriteRenderer.sortingOrder = 0;
    }
}
