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
    [Header("UI References")] [SerializeField]
    private SpriteRenderer cardBackground;

    [SerializeField] private SpriteRenderer cardIcon;
    [SerializeField] private TMP_Text energyCost;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Icon Library")] private CardIconLibrary _iconLibrary;

    [Header("Defaults")] [Tooltip("Shown when CardData does not specify an artwork.")] [SerializeField]
    private Sprite fallbackIcon;

    [Tooltip("Used if no energy value is provided when binding.")] [SerializeField]
    private int defaultEnergyCost = 1;

    [Header("Runtime")] public CardData Data;
    public CardInstance Instance;

    private CardFXHelper _fxHelper;
    private bool _isDragging;
    private PlayerManager _playerManager;

    private void Awake()
    {
        // Find PlayerManager in the scene
        _playerManager = FindFirstObjectByType<PlayerManager>();
        if (_playerManager != null && _playerManager.playerData != null)
        {
            _playerManager.playerData.OnStatsChanged += UpdateVisuals;
        }

        // Auto-wire references if not assigned
        if (cardBackground == null) cardBackground = FindChildByName<SpriteRenderer>("CardBackground");
        if (cardIcon == null) cardIcon = FindChildByName<SpriteRenderer>("CardIcon");
        if (energyCost == null) energyCost = FindChildByName<TMP_Text>("EnergyCost");
        if (cardName == null) cardName = FindChildByName<TMP_Text>("CardName");
        if (descriptionText == null) descriptionText = FindChildByName<TMP_Text>("DescriptionText");

        _iconLibrary = Resources.Load<CardIconLibrary>("Nether/StatusIcons/DefaultCardIconLibrary");

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
            _fxHelper.animHelper.arrowHelper = GetComponent<BezierCardArrowHelper>();
            if (_fxHelper.animHelper.arrowHelper == null)
                _fxHelper.animHelper.arrowHelper = gameObject.AddComponent<BezierCardArrowHelper>();
        }
    }

    private void OnDestroy()
    {
        if (_playerManager != null && _playerManager.playerData != null)
        {
            _playerManager.playerData.OnStatsChanged -= UpdateVisuals;
        }
    }

    public void UpdateVisuals()
    {
        if (Instance != null)
        {
            Bind(Instance);
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

        // Set icon from library
        if (_iconLibrary != null && data != null)
        {
            cardIcon.sprite = _iconLibrary.GetIcon(data.iconCategory);
        }

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
                var summaryParts = new List<string>();
                var processedOps = new HashSet<OperationType>();

                foreach (var effect in instance.rolledEffects)
                {
                    if (processedOps.Contains(effect.operationType)) continue;

                    int total = instance.GetTotal(effect.operationType);
                    if (total == 0 && effect.operationType != OperationType.EndTurn) continue;

                    switch (effect.operationType)
                    {
                        case OperationType.Damage:
                            int strengthBonus = _playerManager != null ? _playerManager.playerData.strength : 0;
                            int finalDamage = total + strengthBonus;
                            string damageColor = strengthBonus > 0 ? "#50C878" : "#FA5053"; // Green if buffed, else red

                            if (effect.targetRule == TargetRule.Self)
                                summaryParts.Add($"Lose {finalDamage} <color=#E51B1B>Health</color>.");
                            else
                                summaryParts.Add($"Inflict <color={damageColor}>{finalDamage}</color> Damage.");
                            break;
                        case OperationType.AddShield:
                            summaryParts.Add($"Gain {total} <color=#57B9FF>Block</color>.");
                            break;
                        case OperationType.Heal:
                            summaryParts.Add($"Heal {total} <color=#50C878>Health</color>.");
                            break;
                        case OperationType.AddEnergy:
                            summaryParts.Add($"Gain {total} <color=#FFD700>Energy</color>.");
                            break;
                        case OperationType.DrawCards:
                            summaryParts.Add($"Draw {total} <color=#4682B4>Cards</color>.");
                            break;
                        case OperationType.AddStrength:
                            summaryParts.Add($"Gain {total} <color=#FF7F50>Strength</color>.");
                            break;
                        case OperationType.EndTurn:
                            summaryParts.Add($"<color=#FFD700>End Turn</color>.");
                            break;
                    }

                    processedOps.Add(effect.operationType);
                }

                baseDesc = string.Join("\n", summaryParts);
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

            // Set icon from library
            if (_iconLibrary != null)
            {
                cardIcon.sprite = _iconLibrary.GetIcon(Data.iconCategory);
            }
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
            energyCost.text = energyVal.ToString();
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
            // Hide arrow immediately when drag ends
            if (_fxHelper.animHelper != null && _fxHelper.animHelper.arrowHelper != null)
            {
                _fxHelper.animHelper.arrowHelper.StopDrawing();
            }
            
            Debug.Log($"[CardRender] OnEndDrag - mouse position: {eventData.position}");
            
            // Check the target rule first
            TargetRule rule = Data.GetDominatingTargetRule();
            Debug.Log($"[CardRender] Card target rule: {rule}");
            
            // For enemy-targeting cards, check if cursor is near the card (not card position)
            // For other cards, check if card itself is near original position
            bool shouldReturnToHand = false;
            
            if (rule == TargetRule.Enemy)
            {
                // For enemy-targeting cards, check if the CURSOR is near the card's position
                // (since the card stays in place, we need to check cursor movement)
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 cardScreenPos = cam.WorldToScreenPoint(transform.position);
                    float cursorDistance = Vector2.Distance(eventData.position, new Vector2(cardScreenPos.x, cardScreenPos.y));
                    
                    // If cursor is close to the card (didn't drag far), return to hand
                    if (cursorDistance <= 100f) // 100 pixels threshold
                    {
                        Debug.Log($"[CardRender] Cursor too close to card ({cursorDistance} pixels) - returning to hand");
                        shouldReturnToHand = true;
                    }
                }
            }
            else
            {
                // For non-enemy targeting cards, use the original position check
                if (_fxHelper.animHelper != null && _fxHelper.animHelper.IsNearOriginalPosition(this))
                {
                    Debug.Log("[CardRender] Card released near original position - returning to hand");
                    shouldReturnToHand = true;
                }
            }
            
            if (shouldReturnToHand)
            {
                _fxHelper.OnCardRelease(this, validTarget: false);

                // Fix layout
                var handViewer = FindFirstObjectByType<DeckViewer>();
                if (handViewer != null)
                    handViewer.RebuildSmart();

                return;
            }

            bool validTarget = false;
            EnemyRender targetEnemy = null;

            switch (rule)
            {
                case TargetRule.Enemy:
                    targetEnemy = GetEnemyOnMouse(eventData.position);
                    if (targetEnemy != null)
                    {
                        Debug.Log($"[CardRender] Valid enemy target found: {targetEnemy.data.enemyName}");
                        validTarget = true;
                    }
                    else
                    {
                        Debug.Log("[CardRender] No enemy target found at release position");
                    }
                    break;

                case TargetRule.Self:
                    Debug.Log("[CardRender] Self-targeting card");
                    validTarget = true;
                    break;
            }

            if (validTarget)
            {
                Debug.Log("[CardRender] Valid target confirmed, attempting to play card");
                var playerManager = FindFirstObjectByType<PlayerManager>();
                if (playerManager != null)
                {
                    bool cardPlayed = playerManager.PlayCard(Data, Instance, targetEnemy);
                    if (!cardPlayed)
                    {
                        Debug.LogWarning("[CardRender] PlayCard returned false - card was not played");
                        validTarget = false;
                    }
                    else
                    {
                        Debug.Log("[CardRender] Card successfully played");
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
                    Debug.LogWarning("[CardRender] PlayerManager not found");
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

        // For 2D games, we need to convert screen to world at the camera's z-plane
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(cam.transform.position.z)));
        
        Debug.Log($"[CardRender] Checking for enemy at screen pos: {screenPosition}, world pos: {worldPos}");

        // Use OverlapPoint to check what's at the mouse cursor position
        Collider2D[] colliders = Physics2D.OverlapPointAll(new Vector2(worldPos.x, worldPos.y));
        
        Debug.Log($"[CardRender] Found {colliders.Length} colliders at mouse position");

        foreach (var collider in colliders)
        {
            if (collider == null) continue;

            // Skip if it's this card's collider
            if (collider.gameObject == gameObject || collider.transform.IsChildOf(transform))
            {
                Debug.Log($"[CardRender] Skipping card's own collider: {collider.name}");
                continue;
            }

            Debug.Log($"[CardRender] Hit: {collider.name}, layer: {LayerMask.LayerToName(collider.gameObject.layer)}");

            // Check if the hit object has an EnemyRender component
            EnemyRender enemyRender = collider.GetComponent<EnemyRender>();
            if (enemyRender != null)
            {
                if (enemyRender.data != null && enemyRender.data.isAlive)
                {
                    Debug.Log($"[CardRender] ✓ Card dropped on ALIVE enemy: {enemyRender.data.enemyName}");
                    return enemyRender;
                }
                else if (enemyRender.data == null)
                {
                    Debug.LogWarning($"[CardRender] Enemy found but has no data: {collider.name}");
                }
                else if (!enemyRender.data.isAlive)
                {
                    Debug.Log($"[CardRender] Enemy found but is dead: {enemyRender.data.enemyName}");
                }
            }
        }

        Debug.Log("[CardRender] No valid enemy found at mouse position");
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