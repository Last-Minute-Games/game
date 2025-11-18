namespace GameItems
{
    using System.Collections;
    using System.Collections.Generic;
    using DG.Tweening;
    using GameItems.Cards;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Splines;

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
        }

        public void Rebuild()
        {
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
            if (splineContainer != null && autoLayoutOnSpline)
            {
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
    }
}
