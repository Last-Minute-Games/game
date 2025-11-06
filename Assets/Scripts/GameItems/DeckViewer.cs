using System.Collections.Generic;
using UnityEngine;
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

    private readonly List<CardRender> _renders = new();

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

        for (int i = 0; i < list.Count; i++)
        {
            var cardData = list[i];
            var go = Instantiate(cardPrefab);

            // Parent and reset transform
            if (content != null)
            {
                var rt = go.GetComponent<RectTransform>();
                if (rt == null) rt = go.AddComponent<RectTransform>();
                rt.SetParent(content, false);
            }

            // Ensure CardRender exists
            var render = go.GetComponent<CardRender>();
            if (render == null) render = go.AddComponent<CardRender>();
            render.Bind(cardData);

            _renders.Add(render);
        }

        // Optional spline placement support (if used in world space)
        if (splineContainer != null && content == null && _renders.Count > 0)
        {
            float tStep = _renders.Count > 1 ? 1f / (_renders.Count - 1) : 0f;
            for (int i = 0; i < _renders.Count; i++)
            {
                var t = (i == _renders.Count - 1) ? 1f : i * tStep;
                var pos = splineContainer.Spline.EvaluatePosition(t);
                _renders[i].transform.position = pos;
            }
        }
    }

    private List<CardData> ResolveCards()
    {
        // Try to get from PlayerManager if available
        if (playerManager != null)
        {
            var cm = playerManager.cardManager;
            switch (source)
            {
                case Source.Hand: return new List<CardData>(cm.hand);
                case Source.DrawPile: return new List<CardData>(cm.drawPile);
                case Source.DiscardPile: return new List<CardData>(cm.discardPile);
                case Source.AllPool: return new List<CardData>(cm.allCardPool);
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
