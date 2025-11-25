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
        [Tooltip("Seconds to tween cards into their new positions/rotations along the spline.")]
        [SerializeField] private float tweenDuration = 0.5f;
        [Tooltip("If true, will layout cards along the spline after building or when calling Rebuild().")]
        [SerializeField] private bool autoLayoutOnSpline = true;

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
        
        private IEnumerator UpdateCardPositions(float duration)
        {
            if (_renders.Count == 0) yield break;

            float cardSpacing = 1f / 10f;
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

                // Using DOTween to animate position and rotation
                _renders[i].transform
                    .DOMove(splinePosition + transform.position + 0.01f * i * Vector3.back, duration)
                    .SetEase(Ease.OutQuad);

                _renders[i].transform
                    .DORotate(rotation.eulerAngles, duration)
                    .SetEase(Ease.OutQuad);
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

                    var render = go.GetComponent<CardRender>();
                    if (render == null) render = go.AddComponent<CardRender>();

                    render.Bind(inst);
                    _renders.Add(render);
                    
                    // Trigger draw FX (animation + sound)
                    var fxHelper = render.GetComponent<CardFXHelper>();
                    if (fxHelper != null)
                    {
                        fxHelper.OnCardDrawn(render);
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
        /// <param name="duration">Time for each card to fly</param>
        /// <param name="staggerDelay">Delay between each card starting</param>
        /// <param name="onComplete">Callback when all cards finish animating</param>
        public void AnimateDiscardAllVisuals(Vector3 discardTargetWorldPos, float duration = 0.4f, float staggerDelay = 0.05f, System.Action onComplete = null)
        {
            if (_renders.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            Debug.Log($"[DeckViewer] Animating {_renders.Count} card visuals to discard pile");

            // Capture the current renders into a separate list so they won't be affected by data changes
            List<CardRender> cardsToAnimate = new List<CardRender>(_renders);
            int totalCards = cardsToAnimate.Count;
            int completedCards = 0;

            // Clear the _renders list immediately so rebuilds won't interfere
            _renders.Clear();

            // Disable card interactions during animation
            CardFXHelper.CardInteraction.Locked = true;

            for (int i = 0; i < cardsToAnimate.Count; i++)
            {
                var card = cardsToAnimate[i];
                if (card == null) continue;

                // Detach from parent so it won't be destroyed when content is cleared
                card.transform.SetParent(null, worldPositionStays: true);

                float delay = i * staggerDelay;

                // Create the animation sequence
                Sequence cardSequence = DOTween.Sequence();
                
                // Delay based on position in hand
                if (delay > 0)
                    cardSequence.AppendInterval(delay);

                // Tween to discard position with arc motion
                cardSequence.Append(card.transform
                    .DOMove(discardTargetWorldPos, duration)
                    .SetEase(Ease.InQuad));

                // Rotate and scale down while flying
                cardSequence.Join(card.transform
                    .DORotate(new Vector3(0, 0, Random.Range(-15f, 15f)), duration)
                    .SetEase(Ease.InOutQuad));

                cardSequence.Join(card.transform
                    .DOScale(0.3f, duration)
                    .SetEase(Ease.InQuad));

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
        /// <param name="duration">Time for each card to fly</param>
        /// <param name="staggerDelay">Delay between each card starting its animation</param>
        /// <param name="onComplete">Callback when all cards have been discarded</param>
        public void AnimateDiscardAll(Vector3 discardTargetWorldPos, float duration = 0.4f, float staggerDelay = 0.05f, System.Action onComplete = null)
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

                // Animate card flying to discard pile
                Sequence cardSequence = DOTween.Sequence();
                
                // Slight delay based on position in hand
                if (delay > 0)
                    cardSequence.AppendInterval(delay);

                // Tween to discard position with arc motion
                cardSequence.Append(card.transform
                    .DOMove(discardTargetWorldPos, duration)
                    .SetEase(Ease.InQuad));

                // Rotate and scale down while flying
                cardSequence.Join(card.transform
                    .DORotate(new Vector3(0, 0, Random.Range(-15f, 15f)), duration)
                    .SetEase(Ease.InOutQuad));

                cardSequence.Join(card.transform
                    .DOScale(0.3f, duration)
                    .SetEase(Ease.InQuad));

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

            // Default discard target: down and to the right
            Vector3 target = discardTarget ?? (transform.position + new Vector3(2f, -3f, 0));
            
            // Use visual-independent animation so it works even if data gets cleared during animation
            AnimateDiscardAllVisuals(target, duration: 0.4f, staggerDelay: 0.05f, onComplete);
        }
    }
}
