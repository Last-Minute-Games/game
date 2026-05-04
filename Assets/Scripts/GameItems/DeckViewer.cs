namespace GameItems
{
    using System.Collections;
    using System.Collections.Generic;
    using DG.Tweening;
    using Cards;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Splines;
    using Cards.Helpers;

    public class DeckViewer : MonoBehaviour
    {
        [Header("Data Source")]
        [SerializeField] private PlayerManager playerManager;

        public enum Source
        {
            UsableCards, // The player's owned deck (PlayerData.usableCards)
            Hand,
            DrawPile,
            DiscardPile,
            AllPool
        }

        [SerializeField] private Source source = Source.UsableCards;

        [Header("UI")]
        [Tooltip("Parent container under a Canvas where card prefabs will be instantiated.")]
        [SerializeField] private RectTransform content;
        [Tooltip("Prefab matching the CardPrefab hierarchy (Wrapper/CardBackground, EnergyCost, CardName, CardIcon, DescriptionText)")]
        [SerializeField] private GameObject cardPrefab;
        [Tooltip("Optional: place spawned cards along this spline (non-UI/worldspace usage). If null, they are simply parented under Content.")]
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private bool buildOnStart = true;

        [Header("Spline Layout")] 
        [Tooltip("Spacing between cards along the spline in normalized t (0..1). Example: 0.1 = 10 cards span the entire spline.")]
        [SerializeField] private float splineCardSpacing = 0.1f; // 1f / 10f as in screenshot
        [Tooltip("Max hand size used for spline distribution (matches OverhaulHandView).")]
        [SerializeField] private float maxHandSize = 10f;
        [Tooltip("Seconds to tween cards into their new positions/rotations along the spline (Overhaul-style default 0.15).")]
        [SerializeField] private float tweenDuration = 0.15f;
        [Tooltip("If true, will layout cards along the spline after building or when calling Rebuild().")]
        [SerializeField] private bool autoLayoutOnSpline = true;

        [Header("Pile anchors (Nether / Overhaul-style)")]
        [Tooltip("Optional: new hand cards spawn here, then tween along the spline. If unset, cards use the default content origin and the draw scale-in effect.")]
        [SerializeField] private Transform drawPilePoint;
        [Tooltip("Optional: end-of-turn discard and ClearSmooth target. If unset, ClearSmooth uses a fixed offset from this viewer's transform.")]
        [SerializeField] private Transform discardPilePoint;

        private readonly List<CardRender> _renders = new();
        private Coroutine _layoutRoutine;

        private void Start()
        {
            if (buildOnStart)
                Rebuild();
        }

        public void SetPlayer(PlayerManager pm)
        {
            playerManager = pm;
        }

        public void SetSource(Source s, bool rebuild = true)
        {
            source = s;
            if (rebuild) Rebuild();
        }

        /// <summary>Wire from code if you prefer not to use the inspector (e.g. shared scene anchors).</summary>
        public void SetPilePointTransforms(Transform drawPoint, Transform discardPoint)
        {
            drawPilePoint = drawPoint;
            discardPilePoint = discardPoint;
        }

        public void Clear()
        {
            for (int i = _renders.Count - 1; i >= 0; i--)
            {
                if (_renders[i] != null)
                    Destroy(_renders[i].gameObject);
            }
            _renders.Clear();

            if (content != null)
            {
                for (int i = content.childCount - 1; i >= 0; i--)
                    Destroy(content.GetChild(i).gameObject);
            }
        }

        private void ApplyDrawPileSpawnIfHand(GameObject go)
        {
            if (drawPilePoint == null || source != Source.Hand)
                return;
            go.transform.SetPositionAndRotation(drawPilePoint.position, drawPilePoint.rotation);
        }

        /// <summary>
        /// Stops any ongoing layout animation (e.g., cards repositioning on spline).
        /// Useful to call before clearing cards to avoid animation conflicts.
        /// </summary>
        public void StopLayoutAnimation()
        {
            if (_layoutRoutine != null)
            {
                Debug.Log("[DeckViewer] Stopping ongoing layout animation");
                StopCoroutine(_layoutRoutine);
                _layoutRoutine = null;
            }
        }
        
        private IEnumerator UpdateCardPositions(float duration)
        {
            if (_renders.Count == 0)
            {
                CardFXHelper.CardInteraction.Locked = false; // Unlock even if no cards
                yield break;
            }

            float cardSpacing = 1f / maxHandSize;
            float firstCardPosition = 0.5f - (_renders.Count - 1) * cardSpacing / 2f;
            var spline = splineContainer.Spline;

            for (int i = 0; i < _renders.Count; i++)
            {
                if (_renders[i] == null) continue;

                float t = firstCardPosition + i * cardSpacing;
                Vector3 splinePosition = spline.EvaluatePosition(t);
                Vector3 forward = spline.EvaluateTangent(t);
                Vector3 up = spline.EvaluateUpVector(t);
                Quaternion rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);

                var cardRender = _renders[i]; // Capture for closure

                // OverhaulHandView-style DOMove / DORotate (no extra ease on layout tweens)
                _renders[i].transform
                    .DOMove(splinePosition + transform.position + 0.01f * i * Vector3.back, duration)
                    .OnComplete(() =>
                    {
                        if (cardRender != null)
                        {
                            var fxHelper = cardRender.GetComponent<CardFXHelper>();
                            if (fxHelper != null && fxHelper.animHelper != null)
                            {
                                fxHelper.animHelper.UpdateOriginalPosition();
                                Debug.Log($"[DeckViewer] OnComplete - Updated original position for card '{cardRender.Data?.name}' to {fxHelper.animHelper.GetOriginalPosition()}");
                            }
                        }
                    });

                _renders[i].transform.DORotate(rotation.eulerAngles, duration);
            }
            yield return new WaitForSeconds(duration);

            CardFXHelper.CardInteraction.Locked = false; // enable card interactions

        }

        /// <summary>
        /// Smart rebuild that only updates cards that changed (removes played cards, adds new ones).
        /// Existing cards stay in place and smoothly rearrange.
        /// </summary>
        public void RebuildSmart()
        {
            CardFXHelper.CardInteraction.Locked = true;
            if (cardPrefab == null)
            {
                Debug.LogWarning("DeckViewer: cardPrefab not assigned");
                return;
            }

            var list = ResolveCards();
            if (list == null)
            {
                Debug.LogWarning("DeckViewer: No card list available.");
                return;
            }

            Debug.Log($"[DeckViewer] Smart rebuild: {list.Count} cards in data, {_renders.Count} currently rendered");

            // For hand cards, use instance-based tracking to handle duplicates
            bool useInstanceTracking = (playerManager != null && source == Source.Hand);
            List<CardInstance> instances = null;
            
            if (useInstanceTracking)
            {
                instances = new List<CardInstance>(playerManager.cardManager.handInstances);
            }

            // Build a list of which renders to keep based on what's in the data
            List<CardRender> rendersToKeep = new List<CardRender>();
            List<int> usedDataIndices = new List<int>();

            // Match existing renders with current data
            for (int i = 0; i < _renders.Count; i++)
            {
                if (_renders[i] == null)
                    continue;

                bool shouldKeep = false;

                if (useInstanceTracking && _renders[i].Instance != null)
                {
                    // Check if this instance still exists in handInstances
                    if (instances.Contains(_renders[i].Instance))
                    {
                        shouldKeep = true;
                        int dataIndex = instances.IndexOf(_renders[i].Instance);
                        usedDataIndices.Add(dataIndex);
                    }
                }
                else
                {
                    // Fall back to data-based matching for non-hand sources
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (!usedDataIndices.Contains(j) && list[j] == _renders[i].Data)
                        {
                            shouldKeep = true;
                            usedDataIndices.Add(j);
                            break;
                        }
                    }
                }

                if (shouldKeep)
                {
                    rendersToKeep.Add(_renders[i]);
                }
                else
                {
                    Debug.Log($"[DeckViewer] Removing card: {_renders[i].Data.name}");
                    Destroy(_renders[i].gameObject);
                }
            }

            _renders.Clear();
            _renders.AddRange(rendersToKeep);

            // Add new cards that aren't rendered yet
            if (useInstanceTracking)
            {
                for (int i = 0; i < instances.Count; i++)
                {
                    if (usedDataIndices.Contains(i))
                        continue; // Already have a render for this instance

                    var inst = instances[i];
                    Debug.Log($"[DeckViewer] Adding new card: {inst.data.name}");
                    
                    var go = Instantiate(cardPrefab);
                    var sortingGroup = go.GetComponent<SortingGroup>();
                    if (sortingGroup != null)
                    {
                        // Use the final position in _renders list for sorting order
                        sortingGroup.sortingOrder = _renders.Count + 100;
                    }

                    if (content != null)
                        go.transform.SetParent(content, false);
                    ApplyDrawPileSpawnIfHand(go);

                    var render = go.GetComponent<CardRender>();
                    if (render == null) render = go.AddComponent<CardRender>();

                    render.Bind(inst);
                    _renders.Add(render);
                    
                    // Trigger draw FX (animation + sound)
                    var fxHelper = render.GetComponent<CardFXHelper>();
                    Debug.Log($"[DeckViewer] Card '{inst.data.name}' - FXHelper found: {fxHelper != null}");
                    if (fxHelper != null)
                    {
                        Debug.Log($"[DeckViewer] Calling OnCardDrawn for '{inst.data.name}'");
                        fxHelper.OnCardDrawn(render, playDrawScaleAnimation: drawPilePoint == null);
                    }
                    else
                    {
                        Debug.LogWarning($"[DeckViewer] FXHelper is NULL for card '{inst.data.name}'!");
                    }
                }
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (usedDataIndices.Contains(i))
                        continue; // Already have a render for this card

                    var cardData = list[i];
                    Debug.Log($"[DeckViewer] Adding new card: {cardData.name}");
                    
                    var go = Instantiate(cardPrefab);
                    var sortingGroup = go.GetComponent<SortingGroup>();
                    if (sortingGroup != null)
                    {
                        // Use the final position in _renders list for sorting order
                        sortingGroup.sortingOrder = _renders.Count + 100;
                    }

                if (content != null)
                    go.transform.SetParent(content, false);
                ApplyDrawPileSpawnIfHand(go);

                var render = go.GetComponent<CardRender>();
                if (render == null) render = go.AddComponent<CardRender>();

                render.Bind(cardData);
                _renders.Add(render);
            }
            }

            // Re-animate all cards to their new positions
            if (splineContainer != null && autoLayoutOnSpline)
            {
                if (_layoutRoutine != null) StopCoroutine(_layoutRoutine);
                _layoutRoutine = StartCoroutine(UpdateCardPositions(tweenDuration));
            }
            else
            {
                // No spline layout, unlock immediately
                CardFXHelper.CardInteraction.Locked = false;
            }
        }

        /// <summary>
        /// Full rebuild - clears everything and rebuilds from scratch.
        /// Use this for initial setup or when switching sources.
        /// </summary>
        public void Rebuild()
        {
            CardFXHelper.CardInteraction.Locked = true; // lock card interaction to prevent malformation

            Clear();
            if (cardPrefab == null)
            {
                Debug.LogWarning("DeckViewer: cardPrefab not assigned");
                return;
            }

            var list = ResolveCards();
            if (list == null)
            {
                Debug.LogWarning("DeckViewer: No card list available.");
                return;
            }
            
            Debug.Log("Resolved " + list.Count + " cards.");

            for (var index = 0; index < list.Count; index++)
            {
                var cardData = list[index];
                var go = Instantiate(cardPrefab);
                
                var sortingGroup = go.GetComponent<SortingGroup>();
                if (sortingGroup != null)
                    sortingGroup.sortingOrder = index + 100;

                // Parent and reset transform (optional)
                if (content != null)
                {
                    go.transform.SetParent(content, false);
                }
                ApplyDrawPileSpawnIfHand(go);

                // Ensure CardRender exists
                var render = go.GetComponent<CardRender>();
                if (render == null) render = go.AddComponent<CardRender>();

                // Bind with runtime instance (rolled values) when viewing hand
                if (playerManager != null && source == Source.Hand)
                {
                    var cm = playerManager.cardManager;
                    CardInstance inst = null;
                    if (cm != null && index < cm.handInstances.Count)
                        inst = cm.handInstances[index];
                    else if (cm != null)
                        inst = cm.GetLatestInstanceFor(cardData);

                    if (inst != null) render.Bind(inst);
                    else render.Bind(cardData);
                }
                else
                {
                    render.Bind(cardData);
                }

                _renders.Add(render);
            }

            // Tween cards along spline if configured
            Debug.Log("splineContainer: " + splineContainer);
            Debug.Log("autoLayoutOnSpline: " + autoLayoutOnSpline);
            
            if (splineContainer != null && autoLayoutOnSpline)
            {
                Debug.Log("Layout cards along spline.");
                if (_layoutRoutine != null) StopCoroutine(_layoutRoutine);
                _layoutRoutine = StartCoroutine(UpdateCardPositions(tweenDuration));
            }
            else
            {
                // No spline layout, unlock immediately
                CardFXHelper.CardInteraction.Locked = false;
            }
        }

        private List<CardData> ResolveCards()
        {
            // Try to get from PlayerManager if available
            if (playerManager != null)
            {
                var cm = playerManager.cardManager;
                
                // Debug.Log(source);
                
                switch (source)
                {
                    case Source.Hand:
                    {
                        var l = new List<CardData>();
                        l.AddRange(cm.hand);
                        return l;
                    }
                    case Source.DrawPile:
                    {
                        var l = new List<CardData>();
                        l.AddRange(cm.drawPile);
                        return l;
                    }
                    case Source.DiscardPile:
                    {
                        var l = new List<CardData>();
                        l.AddRange(cm.discardPile);
                        return l;
                    }
                    case Source.AllPool:
                    {
                        var l = new List<CardData>();
                        l.AddRange(cm.allCardPool);
                        return l;
                    }
                    case Source.UsableCards:
                    default:
                        return playerManager.playerData != null && playerManager.playerData.usableCards != null
                            ? new List<CardData>(playerManager.playerData.usableCards)
                            : null;
                }
            }

            // Fallback: try to load from Resources/Cards if nothing else is wired
            var resources = Resources.LoadAll<CardData>("Cards");
            return resources != null ? new List<CardData>(resources) : null;
        }

        public IReadOnlyList<CardRender> GetRenders() => _renders;

        /// <summary>
        /// Animates currently visible cards to discard without touching the data.
        /// This allows the animation to play independently even if the card data gets cleared.
        /// Perfect for end-of-round animations where data is wiped but visuals should persist.
        /// </summary>
        /// <param name="discardTargetWorldPos">World position where cards fly to</param>
        /// <param name="duration">Time for each card to fly (Overhaul-style default 0.15)</param>
        /// <param name="staggerDelay">Delay between each card starting</param>
        /// <param name="onComplete">Callback when all cards finish animating</param>
        public void AnimateDiscardAllVisuals(Vector3 discardTargetWorldPos, float duration = 0.15f, float staggerDelay = 0.05f, System.Action onComplete = null)
        {
            if (_renders.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            Debug.Log($"[DeckViewer] Animating {_renders.Count} card visuals to discard pile");

            // Capture the current renders into a separate list so they won't be affected by data changes
            List<CardRender> cardsToAnimate = new List<CardRender>(_renders);
            
            // Filter out null cards upfront
            cardsToAnimate.RemoveAll(card => card == null);
            
            if (cardsToAnimate.Count == 0)
            {
                Debug.LogWarning("[DeckViewer] All cards were null, skipping animation");
                _renders.Clear();
                onComplete?.Invoke();
                return;
            }
            
            int totalCards = cardsToAnimate.Count;
            int completedCards = 0;

            // Hide all arrows and reset card states before animating discard
            foreach (var card in cardsToAnimate)
            {
                if (card == null) continue;
                
                var arrowHelper = card.GetComponent<BezierCardArrowHelper>();
                if (arrowHelper != null)
                {
                    arrowHelper.StopDrawing();
                }
                
                // Clear enemy hover sprites
                var animHelper = card.GetComponent<CardAnimationHelper>();
                if (animHelper != null)
                {
                    animHelper.ClearEnemyHoverSprites();
                }
                
                // Exit hover state and reset scale to prevent exponential size during discard
                var fxHelper = card.GetComponent<CardFXHelper>();
                if (fxHelper != null)
                {
                    fxHelper.OnCardExit(card);
                }
                
                // Kill any existing tweens on this card to prevent conflicts
                card.transform.DOKill();
            }

            // Clear the _renders list immediately so rebuilds won't interfere
            _renders.Clear();

            // Disable card interactions during animation
            CardFXHelper.CardInteraction.Locked = true;

            for (int i = 0; i < cardsToAnimate.Count; i++)
            {
                var card = cardsToAnimate[i];
                if (card == null)
                {
                    // If card is null, still count it as completed to avoid lock-up
                    completedCards++;
                    if (completedCards >= totalCards)
                    {
                        CardFXHelper.CardInteraction.Locked = false;
                        Debug.Log("[DeckViewer] All card visuals discarded (with nulls)");
                        onComplete?.Invoke();
                    }
                    continue;
                }

                // Detach from parent so it won't be destroyed when content is cleared
                card.transform.SetParent(null, worldPositionStays: true);

                float delay = i * staggerDelay;

                // OverhaulCardSystem-style: DOMove + DOScale to zero in parallel
                Sequence cardSequence = DOTween.Sequence();
                
                if (delay > 0)
                    cardSequence.AppendInterval(delay);

                cardSequence.Append(card.transform.DOMove(discardTargetWorldPos, duration));
                cardSequence.Join(card.transform.DOScale(Vector3.zero, duration).SetEase(Ease.InQuad));

                // Destroy after animation completes
                cardSequence.OnComplete(() =>
                {
                    if (card != null)
                        Destroy(card.gameObject);

                    completedCards++;
                    if (completedCards >= totalCards)
                    {
                        CardFXHelper.CardInteraction.Locked = false;
                        Debug.Log("[DeckViewer] All card visuals discarded and destroyed");
                        onComplete?.Invoke();
                    }
                });
            }
        }

        /// <summary>
        /// Animates all cards flying to a discard position with a stagger effect, then clears them.
        /// Call this when the round ends for a smooth card game feel.
        /// </summary>
        /// <param name="discardTargetWorldPos">World position where cards fly to (usually discard pile)</param>
        /// <param name="duration">Time for each card to fly (Overhaul-style default 0.15)</param>
        /// <param name="staggerDelay">Delay between each card starting its animation</param>
        /// <param name="onComplete">Callback when all cards have been discarded</param>
        public void AnimateDiscardAll(Vector3 discardTargetWorldPos, float duration = 0.15f, float staggerDelay = 0.05f, System.Action onComplete = null)
        {
            if (_renders.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            Debug.Log($"[DeckViewer] Animating {_renders.Count} cards to discard pile");

            int totalCards = _renders.Count;
            int completedCards = 0;

            for (int i = 0; i < _renders.Count; i++)
            {
                if (_renders[i] == null) continue;

                var card = _renders[i];
                float delay = i * staggerDelay;

                Sequence cardSequence = DOTween.Sequence();
                
                if (delay > 0)
                    cardSequence.AppendInterval(delay);

                cardSequence.Append(card.transform.DOMove(discardTargetWorldPos, duration));
                cardSequence.Join(card.transform.DOScale(Vector3.zero, duration).SetEase(Ease.InQuad));

                // Destroy after animation
                cardSequence.OnComplete(() =>
                {
                    if (card != null)
                        Destroy(card.gameObject);

                    completedCards++;
                    if (completedCards >= totalCards)
                    {
                        _renders.Clear();
                        Debug.Log("[DeckViewer] All cards discarded and cleared");
                        onComplete?.Invoke();
                    }
                });
            }
        }

        /// <summary>
        /// Smooth clear - animates cards out before clearing.
        /// Use this instead of Clear() for end-of-turn visual polish.
        /// Uses visual-independent animation so it works even if card data gets cleared.
        /// </summary>
        public void ClearSmooth(Vector3? discardTarget = null, System.Action onComplete = null)
        {
            if (_renders.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            Vector3 target = discardTarget
                ?? (discardPilePoint != null
                    ? discardPilePoint.position
                    : (transform.position + new Vector3(2f, -3f, 0)));
            
            AnimateDiscardAllVisuals(target, duration: 0.15f, staggerDelay: 0.05f, onComplete);
        }
    }
}
