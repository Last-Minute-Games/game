using UnityEngine;

namespace GameItems.Cards.Helpers
{
    /// <summary>
    /// Singleton manager that tracks global card state across all card instances.
    /// This ensures hover and selection tracking works properly across different cards.
    /// </summary>
    public class CardFXManager : MonoBehaviour
    {
        private static CardFXManager _instance;
        public static CardFXManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CardFXManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CardFXManager");
                        _instance = go.AddComponent<CardFXManager>();
                    }
                }
                return _instance;
            }
        }

        // ────────────────────────────────
        // Shared State Across All Cards
        // ────────────────────────────────
        
        // Track which card is currently hovered (only one at a time)
        private CardRender _currentlyHoveredCard;
        public CardRender CurrentlyHoveredCard
        {
            get => _currentlyHoveredCard;
            set => _currentlyHoveredCard = value;
        }

        // Track which card is currently selected (only one at a time)
        private CardRender _currentlySelectedCard;
        public CardRender CurrentlySelectedCard
        {
            get => _currentlySelectedCard;
            set => _currentlySelectedCard = value;
        }

        // Track if draw sound has been played this round (shared across all cards)
        private bool _drawSoundPlayed;
        public bool DrawSoundPlayed
        {
            get => _drawSoundPlayed;
            set => _drawSoundPlayed = value;
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Reset the draw sound flag at the start of a new round
        /// </summary>
        public void ResetDrawSoundFlag()
        {
            Debug.Log($"[CardFXManager] Draw sound flag reset. Before: {_drawSoundPlayed}");
            _drawSoundPlayed = false;
            Debug.Log($"[CardFXManager] Draw sound flag reset complete. After: {_drawSoundPlayed}");
        }

        /// <summary>
        /// Clear all tracked state (call this when cleaning up cards)
        /// </summary>
        public void ClearAllState()
        {
            _currentlyHoveredCard = null;
            _currentlySelectedCard = null;
            _drawSoundPlayed = false;
        }
    }
}

