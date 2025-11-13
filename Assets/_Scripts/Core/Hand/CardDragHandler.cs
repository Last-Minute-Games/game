using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.Gameplay
{
    public class CardDragHandler : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Vector3 startPos;
        private Quaternion originalRotation;
        private CardView cardView;
        private SpriteRenderer spriteRenderer;

        private bool isDragging = false;
        private bool pointerOnCard = false;

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
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0;

            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null && hit.gameObject == gameObject)
            {
                pointerOnCard = true;
                startPos = transform.position;
                originalRotation = transform.rotation;
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
            transform.DORotateQuaternion(Quaternion.identity, 0.2f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0;
            transform.position = worldPos;

            HighlightNearestEnemy(worldPos);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var cardRunner = cardView.transform.GetComponent<CardRunner>();
            var cardData = cardRunner.data;

            var plrCollider = GameObject.Find("Player").GetComponent<BoxCollider2D>();
            
            if (!isDragging) return;

            isDragging = false;
            pointerOnCard = false;
            transform.DOScale(1f, 0.1f);

            var wasEnabled = selfCol.enabled;
            selfCol.enabled = false;

            Vector3 pos = transform.position;
            Collider2D target = Physics2D.OverlapPoint(pos);

            selfCol.enabled = wasEnabled;
            
            if (cardData != null && cardView != null)
            {
                if (cardData.targetingRule.name == "Self Targeting")
                {
                    var distanceToOriginal = (pos - startPos).magnitude;
                    // Debug.Log(distanceToOriginal);

                    if (distanceToOriginal >= 2.5f)
                    {
                        cardView.UseCard(plrCollider);
                        return;   
                    }
                } else if (cardData.targetingRule.name == "Enemy Targeting")
                {
                    if (target != null && target.gameObject != gameObject)
                    {
                        if (cardView.UseCard(target))
                        {
                            ResetEnemyTints();
                            return;
                        }
                    }
                }
            }

            // Return to original position and rotation if not used
            transform.DOMove(startPos, 0.2f);
            transform.DORotateQuaternion(originalRotation, 0.2f);
            ResetEnemyTints();
        }

        private void HighlightNearestEnemy(Vector3 pos)
        {
            // Existing highlight logic...
        }

        private void ResetEnemyTints()
        {
            // Existing reset logic...
        }
    }
}
