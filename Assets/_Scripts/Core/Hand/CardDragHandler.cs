using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.Gameplay
{
    public class CardDragHandler : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Vector3 startPos;
        private CardView cardView;
        private SpriteRenderer spriteRenderer;

        private bool isDragging = false;
        private bool pointerOnCard = false;
    
        private Vector3 screenPoint;
        private BoxCollider2D selfCol;
        
        public bool IsDragging => isDragging;
    
        private void Awake()
        {
            cardView = GetComponent<CardView>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            selfCol = GetComponent<BoxCollider2D>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // check if mouse in on card
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0;
        
            Debug.Log(worldPos);
        
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null && hit.gameObject == gameObject)
            {
                pointerOnCard = true;
                startPos = transform.position;
                spriteRenderer.sortingOrder = 500;
                transform.DOScale(1.15f, 0.1f);
            }   
            else
            {
                pointerOnCard = false;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!pointerOnCard) return;
            isDragging = true;
            // startPos = transform.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0;
            transform.position = worldPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log(isDragging);
            
            if (!isDragging) return;
        
            isDragging = false;
            pointerOnCard = false;                            // reset for the next interaction
            transform.DOScale(1f, 0.1f);
            
            var wasEnabled = selfCol.enabled;
            selfCol.enabled = false;

            Vector3 pos = transform.position; 
            Collider2D target = Physics2D.OverlapPoint(pos);
            
            selfCol.enabled = wasEnabled;  

            if (target != null && target.gameObject != gameObject && cardView != null)
            {
                if (cardView.UseCard(target)) return;
            }

            // Snap back if invalid
            transform.DOMove(startPos, 0.2f).SetEase(Ease.OutCubic)
                .OnComplete(() => spriteRenderer.sortingOrder = 0);


            foreach (var e in FindObjectsOfType<Enemy>())
            {
                var sr = e.GetComponent<SpriteRenderer>();
                if (sr) sr.color = Color.white;
            }
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        transform.position = worldPos;

        HighlightNearestEnemy(worldPos);
    }

    private void HighlightNearestEnemy(Vector3 cardPos)
    {
        BattlefieldLayout layout = FindFirstObjectByType<BattlefieldLayout>();
        if (layout == null) return;

        Enemy nearest = null;
        float minDist = float.MaxValue;

        foreach (var e in layout.GetEnemies())
        {
            if (e == null) continue;
            float d = Vector2.Distance(cardPos, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = e;
            }
            // clear tint for all
            var sr = e.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.white;
        }

        if (nearest != null)
        {
            var sr = nearest.GetComponent<SpriteRenderer>();
            if (sr) sr.color = Color.yellow; // highlight target
        }
    }
}
