using System.Collections.Generic;
using GameItems;
using GameItems.Cards;
using GameItems.Cards.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// CardRender populates the UI for a single card instance using a prefab with the
// following expected hierarchy (names can be adjusted, but these are auto-detected):
// CardPrefab
//  └─ Wrapper
//      ├─ CardBackground (Image)
//      ├─ EnergyCost     (TMP_Text)
//      ├─ CardName       (TMP_Text)
//      ├─ CardIcon       (Image)
//      └─ DescriptionText(TMP_Text)
//
// You can either assign the fields in the inspector, or leave them null and CardRender
// will attempt to find them by name among the children (case-insensitive contains check).
public class CardRender : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("UI References")] 
    [SerializeField] private SpriteRenderer cardBackground;
    [SerializeField] private SpriteRenderer cardIcon;
    [SerializeField] private TMP_Text energyCost;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Defaults")] 
    [Tooltip("Shown when CardData does not specify an artwork.")]
    [SerializeField] private Sprite fallbackIcon;
    [Tooltip("Used if no energy value is provided when binding.")]
    [SerializeField] private int defaultEnergyCost = 1;

    [Header("Runtime")] 
    public CardData Data;
    public CardInstance Instance;
    
    private CardFXHelper _fxHelper;
    private bool _isDragging;

    private void Awake()
    {
        // Auto-wire references if not assigned
        if (cardBackground == null) cardBackground = FindChildByName<SpriteRenderer>("CardBackground");
        if (cardIcon == null) cardIcon = FindChildByName<SpriteRenderer>("CardIcon");
        if (energyCost == null) energyCost = FindChildByName<TMP_Text>("EnergyCost");
        if (cardName == null) cardName = FindChildByName<TMP_Text>("CardName");
        if (descriptionText == null) descriptionText = FindChildByName<TMP_Text>("DescriptionText");
        
        // Setup CardFXHelper and its sub-helpers
        _fxHelper = GetComponent<CardFXHelper>();
        if (_fxHelper == null)
        {
            _fxHelper = gameObject.AddComponent<CardFXHelper>();
        }
        
        // Ensure sub-helpers are assigned
        if (_fxHelper.sfxHelper == null)
        {
            _fxHelper.sfxHelper = GetComponent<CardSFXHelper>();
            if (_fxHelper.sfxHelper == null)
                _fxHelper.sfxHelper = gameObject.AddComponent<CardSFXHelper>();
        }
        
        if (_fxHelper.animHelper == null)
        {
            _fxHelper.animHelper = GetComponent<CardAnimationHelper>();
            if (_fxHelper.animHelper == null)
                _fxHelper.animHelper = gameObject.AddComponent<CardAnimationHelper>();
        }
        
        // Setup arrow helper for animation helper
        if (_fxHelper.animHelper != null && _fxHelper.animHelper.arrowHelper == null)
        {
            _fxHelper.animHelper.arrowHelper = GetComponent<CardArrowHelper>();
            if (_fxHelper.animHelper.arrowHelper == null)
                _fxHelper.animHelper.arrowHelper = gameObject.AddComponent<CardArrowHelper>();
        }
    }

    public void Bind(CardData data)
    {
        Instance = null;
        Data = data;
        // Name/Description
        if (cardName != null) cardName.text = data != null ? data.name : string.Empty;
        if (descriptionText != null) descriptionText.text = data != null ? data.description : string.Empty;

        cardBackground.sprite = data != null ? data.artwork : null;
        cardIcon.sprite = data != null ? data.icon : null;
        
        // Energy
        int energyVal = data.energyCost;
        if (energyCost != null)
        {
            energyCost.text = energyVal.ToString();
        }
    }

    public void Bind(CardInstance instance, int? energy = null)
    {
        Instance = instance;
        Data = instance != null ? instance.data : null;

        // Name with variability tier prefix if present
        if (cardName)
        {
            if (instance != null && instance.tier.HasValue && Data != null)
            {
                string prefix = Data.GetColoredPrefix(instance.tier.Value);
                cardName.text = string.IsNullOrEmpty(prefix) ? Data.name : $"{prefix} {Data.name}";
            }
            else
            {
                cardName.text = Data != null ? Data.name : string.Empty;
            }
        }

        // Description: keep base text; optionally append rolled summary for clarity
        if (descriptionText != null)
        {
            string baseDesc = string.Empty;
            if (instance != null && instance.rolledEffects != null && instance.rolledEffects.Count > 0)
            {
                int dmg = instance.GetTotal(OperationType.Damage);
                int blk = instance.GetTotal(OperationType.AddShield);
                int heal = instance.GetTotal(OperationType.Heal);
                int addEnergy = instance.GetTotal(OperationType.AddEnergy);
                
                // Check if EndTurn operation exists
                bool hasEndTurn = false;
                foreach (var effect in instance.rolledEffects)
                {
                    if (effect.operationType == OperationType.EndTurn)
                    {
                        hasEndTurn = true;
                        break;
                    }
                }
                
                string summary = string.Empty;
                if (dmg != 0) summary += $"Inflict {dmg} <color=#FA5053>Damage</color>.";
                if (blk != 0) summary += (summary.Length > 0 ? "\n" : "") + $"Gain {blk} <color=#57B9FF>Block</color>.";
                if (heal != 0) summary += (summary.Length > 0 ? "\n" : "") + $"Heal {heal} <color=#50C878>Health</color>.";
                if (addEnergy != 0) summary += (summary.Length > 0 ? "\n" : "") + $"Gain {addEnergy} <color=#FFD700>Energy</color>.";
                if (hasEndTurn) summary += (summary.Length > 0 ? "\n" : "") + $"<color=#FFD700>End Turn</color>.";
                if (!string.IsNullOrEmpty(summary)) baseDesc = summary.Trim();
            }
            descriptionText.text = baseDesc;
        }

        // Sprites from data - use tier-specific artwork if available
        if (Data != null)
        {
            if (instance != null && instance.tier.HasValue)
            {
                cardBackground.sprite = Data.GetArtworkForTier(instance.tier.Value);
            }
            else
            {
                cardBackground.sprite = Data.artwork;
            }
            cardIcon.sprite = Data.icon;
        }
        else
        {
            cardBackground.sprite = null;
            cardIcon.sprite = null;
        }

        // Energy from CardData
        int energyVal = energy ?? (Data != null ? Data.energyCost : defaultEnergyCost);
        if (energyCost != null)
        {
            energyCost.text = energyVal > 0 ? energyVal.ToString() : string.Empty;
        }
    }

    // ─────────────────────────────────────────────
    // Pointer & Mouse listeners to drive CardFXHelper
    // ─────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_fxHelper != null && !_isDragging)
        {
            _fxHelper.OnCardHover(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_fxHelper != null && !_isDragging)
        {
            _fxHelper.OnCardHoverExit(this);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_fxHelper != null)
        {
            _fxHelper.OnCardSelect(this);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // If we didn't drag (OnEndDrag not called), reset the card
        if (!_isDragging && _fxHelper != null)
        {
            // Simply exit hover - OnPointerEnter will handle re-hovering if needed
            _fxHelper.OnCardHoverExit(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        if (_fxHelper != null)
        {
            // Don't update original position - keep the one from OnPointerDown
            _fxHelper.OnCardSelect(this, updatePosition: false);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_fxHelper != null)
        {
            _fxHelper.OnCardDrag(this, eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;

        if (_fxHelper != null)
        {
            // First check if card is near its original position - if so, return it to hand
            if (_fxHelper.animHelper != null && _fxHelper.animHelper.IsNearOriginalPosition(this))
            {
                Debug.Log("[CardRender] Card released near original position - returning to hand");
                _fxHelper.OnCardRelease(this, validTarget: false);
                
                // Fix layout
                var handViewer = FindFirstObjectByType<DeckViewer>();
                if (handViewer != null)
                    handViewer.RebuildSmart();
                
                return;
            }

            bool validTarget = false;
            TargetRule rule = Data.GetDominatingTargetRule();

            EnemyRender targetEnemy = null;

            switch (rule)
            {
                case TargetRule.Enemy:
                    targetEnemy = GetEnemyOnMouse(eventData.position);
                    if (targetEnemy != null)
                        validTarget = true;
                    break;

                case TargetRule.Self:
                    validTarget = true;
                    break;
            }

            if (validTarget)
            {
                var playerManager = FindFirstObjectByType<PlayerManager>();
                if (playerManager != null)
                {
                    bool cardPlayed = playerManager.PlayCard(Data, Instance, targetEnemy);
                    if (!cardPlayed)
                        validTarget = false;
                    else
                    {
                        // Check if card has EndTurn effect - if so, don't rebuild as EndPlayerTurn handles it
                        bool cardHasEndTurn = CheckForEndTurnEffect();
                        
                        if (!cardHasEndTurn)
                        {
                            var roundManager = FindFirstObjectByType<RoundManager>();
                            if (roundManager != null && roundManager.handViewer != null)
                                roundManager.handViewer.RebuildSmart();
                        }
                    }
                }
                else
                {
                    validTarget = false;
                }
            }

            _fxHelper.OnCardRelease(this, validTarget);

            // ALWAYS FIX LAYOUT AFTER DRAG - unless EndTurn was triggered
            bool hasEndTurn = CheckForEndTurnEffect();
            if (!hasEndTurn)
            {
                var handViewer = FindFirstObjectByType<DeckViewer>();
                if (handViewer != null)
                    handViewer.RebuildSmart();
            }
        }
    }

    private EnemyRender GetEnemyOnMouse(Vector2 screenPosition)
    {
        // Convert screen position to world position
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[CardRender] Camera.main is null, cannot check enemy collision.");
            return null;
        }

        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, cam.nearClipPlane));
        
        // Raycast at the drop position to check for enemy colliders, ignoring the card itself
        RaycastHit2D[] hits = new RaycastHit2D[10];
        ContactFilter2D filter = new ContactFilter2D();
        
        Physics2D.Raycast(worldPos, Vector2.zero, filter, hits);
        
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            
            // Skip if it's this card's collider
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                continue;
            
            Debug.Log($"Hit: {hit.collider.name}");
            
            // Check if the hit object has an EnemyRender component
            EnemyRender enemyRender = hit.collider.GetComponent<EnemyRender>();
            if (enemyRender != null && enemyRender.data is { isAlive: true })
            {
                Debug.Log($"[CardRender] Card dropped on enemy: {enemyRender.data.enemyName}");
                return enemyRender;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if this card has an EndTurn effect
    /// </summary>
    private bool CheckForEndTurnEffect()
    {
        if (Data == null) return false;
        
        // Check in CardData effects
        if (Data.effects != null)
        {
            foreach (var effect in Data.effects)
            {
                if (effect.operationType == OperationType.EndTurn)
                    return true;
            }
        }
        
        // Also check instance rolled effects if available
        if (Instance != null && Instance.rolledEffects != null)
        {
            foreach (var effect in Instance.rolledEffects)
            {
                if (effect.operationType == OperationType.EndTurn)
                    return true;
            }
        }
        
        return false;
    }

    // Optional support for non-UI hover via physics raycast (if collider present)
    private void OnMouseOver()
    {
        if (_fxHelper != null && !_isDragging)
        {
            _fxHelper.OnCardHover(this);
        }
    }

    private void OnMouseExit()
    {
        // No-op; exit visuals are handled elsewhere (e.g., release/cancel)
    }

    public void SetEnergy(int value)
    {
        if (energyCost != null) energyCost.text = value.ToString();
    }

    public void SetBackground(Sprite bg)
    {
        if (cardBackground == null) return;
        cardBackground.sprite = bg;
        cardBackground.enabled = bg != null;
    }

    public void SetIcon(Sprite sprite)
    {
        if (cardIcon == null) return;
        cardIcon.sprite = sprite;
        cardIcon.enabled = sprite != null;
    }

    private T FindChildByName<T>(string containsName) where T : Component
    {
        var comps = GetComponentsInChildren<T>(true);
        containsName = containsName.ToLowerInvariant();
        foreach (var c in comps)
        {
            if (c.name.ToLowerInvariant().Contains(containsName))
                return c;
        }
        return null;
    }
}
