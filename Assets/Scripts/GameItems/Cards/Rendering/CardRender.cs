using System.Collections.Generic;
using Entities.Enemies.Render;
using GameItems;
using GameItems.Cards;
using GameItems.Cards.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// CardRender binds UI for a card; description now uses placeholder substitution.
// Restores full drag-release logic so enemies can be targeted and cards get applied.
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

    [Header("Icon Library")]
    private CardIconLibrary _iconLibrary;

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
    private PlayerManager _playerManager;
    
    // Performance: Cached references
    private Camera _mainCamera;
    private DeckViewer _handViewer;
    private RoundManager _roundManager;
    private Collider2D[] _colliderBuffer = new Collider2D[10]; // Reusable buffer for NonAlloc

    private void Awake()
    {
        // Performance: Cache Camera.main (with fallback support for engine upgrades)
        _mainCamera = Camera.main;
        
        // Performance: Cache manager references once
        _playerManager = FindFirstObjectByType<PlayerManager>();
        _roundManager = FindFirstObjectByType<RoundManager>();
        _handViewer = FindFirstObjectByType<DeckViewer>();
        
        if (_playerManager != null && _playerManager.playerData != null)
            _playerManager.playerData.OnStatsChanged += UpdateVisuals;

        // Performance: Optimize GetComponentsInChildren - get all once, then search
        if (cardBackground == null || cardIcon == null || energyCost == null || cardName == null || descriptionText == null)
        {
            var spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            var tmpTexts = GetComponentsInChildren<TMP_Text>(true);
            
            if (cardBackground == null) cardBackground = FindInArray(spriteRenderers, "CardBackground");
            if (cardIcon == null) cardIcon = FindInArray(spriteRenderers, "CardIcon");
            if (energyCost == null) energyCost = FindInArray(tmpTexts, "EnergyCost");
            if (cardName == null) cardName = FindInArray(tmpTexts, "CardName");
            if (descriptionText == null) descriptionText = FindInArray(tmpTexts, "DescriptionText");
        }

        _iconLibrary = Resources.Load<CardIconLibrary>("Nether/StatusIcons/DefaultCardIconLibrary");

        _fxHelper = GetComponent<CardFXHelper>();
        if (_fxHelper == null)
        {
            Debug.LogWarning("[CardRender] CardFXHelper not found on prefab, adding dynamically");
            _fxHelper = gameObject.AddComponent<CardFXHelper>();
        }
        else
        {
            Debug.Log($"[CardRender] CardFXHelper found on prefab. sfxHelper: {_fxHelper.sfxHelper != null}");
        }

        if (_fxHelper.sfxHelper == null)
        {
            Debug.LogWarning("[CardRender] CardSFXHelper is null on CardFXHelper, attempting to find it");
            _fxHelper.sfxHelper = GetComponent<CardSFXHelper>();
            if (_fxHelper.sfxHelper == null)
            {
                Debug.LogWarning("[CardRender] CardSFXHelper not found on GameObject, adding new one (drawCue will be missing!)");
                _fxHelper.sfxHelper = gameObject.AddComponent<CardSFXHelper>();
            }
            else
            {
                Debug.Log($"[CardRender] Found CardSFXHelper. drawCue assigned: {_fxHelper.sfxHelper.drawCue != null}");
            }
        }
        else
        {
            Debug.Log($"[CardRender] CardSFXHelper already assigned. drawCue assigned: {_fxHelper.sfxHelper.drawCue != null}");
        }

        if (_fxHelper.animHelper == null)
        {
            _fxHelper.animHelper = GetComponent<CardAnimationHelper>();
            if (_fxHelper.animHelper == null)
                _fxHelper.animHelper = gameObject.AddComponent<CardAnimationHelper>();
        }

        if (_fxHelper.animHelper != null && _fxHelper.animHelper.arrowHelper == null)
        {
            _fxHelper.animHelper.arrowHelper = GetComponent<BezierCardArrowHelper>();
            if (_fxHelper.animHelper.arrowHelper == null)
                _fxHelper.animHelper.arrowHelper = gameObject.AddComponent<BezierCardArrowHelper>();
        }
    }

    // Performance: Helper to get camera with fallback (handles engine upgrade timing issues)
    private Camera GetCamera()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;
        return _mainCamera;
    }

    private void OnDestroy()
    {
        if (_playerManager != null && _playerManager.playerData != null)
            _playerManager.playerData.OnStatsChanged -= UpdateVisuals;
    }

    public void UpdateVisuals()
    {
        if (Instance != null)
            Bind(Instance);
    }

    public void Bind(CardData data)
    {
        Instance = null;
        Data = data;

        if (cardName != null) cardName.text = data != null ? data.name : string.Empty;

        // Description via base-value substitution (no instance)
        if (descriptionText != null)
        {
            descriptionText.text = data != null
                ? data.BuildDescriptionWithSubstitutions(data.effects)
                : string.Empty;
        }

        cardBackground.sprite = data != null ? data.artwork : null;

        if (_iconLibrary != null && data != null)
            cardIcon.sprite = _iconLibrary.GetIcon(data.iconCategory);

        int energyVal = data != null ? data.energyCost : defaultEnergyCost;
        if (energyCost != null)
            energyCost.text = energyVal.ToString();
    }

    public void Bind(CardInstance instance, int? energy = null)
    {
        Instance = instance;
        Data = instance != null ? instance.data : null;

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

        // Description via final rolled + Strength-buffed values (from instance)
        if (descriptionText != null)
        {
            descriptionText.text = Data != null
                ? Data.BuildDescriptionWithSubstitutionsFromInstance(instance, _playerManager)
                : string.Empty;
        }

        if (Data != null)
        {
            cardBackground.sprite = instance != null && instance.tier.HasValue
                ? Data.GetArtworkForTier(instance.tier.Value)
                : Data.artwork;

            if (_iconLibrary != null)
                cardIcon.sprite = _iconLibrary.GetIcon(Data.iconCategory);
        }
        else
        {
            cardBackground.sprite = null;
            cardIcon.sprite = null;
        }

        int energyVal = energy ?? (Data != null ? Data.energyCost : defaultEnergyCost);
        if (energyCost != null)
            energyCost.text = energyVal.ToString();
    }

    // ─────────────────────────────────────────────
    // Pointer & Mouse listeners to drive CardFXHelper
    // ─────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_fxHelper != null && !_isDragging)
            _fxHelper.OnCardHover(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0)) return;  // <-- FIX
        if (_isDragging) return;
        
        _fxHelper?.OnCardHoverExit(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_fxHelper != null)
            _fxHelper.OnCardSelect(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDragging && _fxHelper != null)
            _fxHelper.OnCardHoverExit(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;

        if (_fxHelper != null)
        {
            // Always do the normal drag-select logic
            _fxHelper.OnCardSelect(this, updatePosition: false);

            // If this card targets enemies, and arrow didn't start because of double-click ---
            TargetRule rule = Data.GetDominatingTargetRule();
            if (rule == TargetRule.Enemy)
            {
                if (_fxHelper.animHelper != null && _fxHelper.animHelper.arrowHelper != null)
                {
                    _fxHelper.animHelper.arrowHelper.StartDrawing();
                }
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_fxHelper != null)
            _fxHelper.OnCardDrag(this, eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;

        if (_fxHelper != null)
        {
            if (_fxHelper.animHelper != null && _fxHelper.animHelper.arrowHelper != null)
                _fxHelper.animHelper.arrowHelper.StopDrawing();

            Debug.Log($"[CardRender] OnEndDrag - mouse position: {eventData.position}");

            TargetRule rule = Data.GetDominatingTargetRule();
            Debug.Log($"[CardRender] Card target rule: {rule}");

            bool shouldReturnToHand = false;

            if (rule == TargetRule.Enemy)
            {
                // Performance: Use cached camera with fallback
                Camera cam = GetCamera();
                if (cam != null)
                {
                    Vector3 cardScreenPos = cam.WorldToScreenPoint(transform.position);
                    float cursorDistance = Vector2.Distance(eventData.position, new Vector2(cardScreenPos.x, cardScreenPos.y));

                    if (cursorDistance <= 100f)
                    {
                        Debug.Log($"[CardRender] Cursor too close to card ({cursorDistance} pixels) - returning to hand");
                        shouldReturnToHand = true;
                    }
                }
            }
            else
            {
                if (_fxHelper.animHelper != null && _fxHelper.animHelper.IsNearOriginalPosition(this))
                {
                    Debug.Log("[CardRender] Card released near original position - returning to hand");
                    shouldReturnToHand = true;
                }
            }

            if (shouldReturnToHand)
            {
                _fxHelper.OnCardRelease(this, validTarget: false);

                // Performance: Use cached hand viewer
                if (_handViewer != null)
                    _handViewer.RebuildSmart();

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
                // Performance: Use cached player manager
                if (_playerManager != null)
                {
                    bool cardPlayed = _playerManager.PlayCard(Data, Instance, targetEnemy);
                    if (!cardPlayed)
                    {
                        Debug.LogWarning("[CardRender] PlayCard returned false - card was not played");
                        validTarget = false;
                    }
                    else
                    {
                        Debug.Log("[CardRender] Card successfully played");

                        if (_fxHelper != null && _fxHelper.animHelper != null)
                            _fxHelper.animHelper.ClearEnemyHoverSprites();

                        bool cardHasEndTurn = CheckForEndTurnEffect();

                        if (!cardHasEndTurn)
                        {
                            // Performance: Use cached round manager
                            if (_roundManager != null && _roundManager.handViewer != null)
                                _roundManager.handViewer.RebuildSmart();
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

            bool hasEndTurn = CheckForEndTurnEffect();
            if (!hasEndTurn)
            {
                // Performance: Use cached hand viewer
                if (_handViewer != null)
                    _handViewer.RebuildSmart();
            }
        }
    }

    private EnemyRender GetEnemyOnMouse(Vector2 screenPosition)
    {
        // Performance: Use cached camera with fallback
        Camera cam = GetCamera();
        if (cam == null)
        {
            Debug.LogWarning("[CardRender] Camera.main is null, cannot check enemy collision.");
            return null;
        }

        Vector3 worldPos = cam.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(cam.transform.position.z)));

        Debug.Log($"[CardRender] Checking for enemy at screen pos: {screenPosition}, world pos: {worldPos}");

        // Performance: Use NonAlloc version to avoid garbage
        int count = Physics2D.OverlapPointNonAlloc(new Vector2(worldPos.x, worldPos.y), _colliderBuffer);
        Debug.Log($"[CardRender] Found {count} colliders at mouse position");

        for (int i = 0; i < count; i++)
        {
            var collider = _colliderBuffer[i];
            if (collider == null) continue;

            if (collider.gameObject == gameObject || collider.transform.IsChildOf(transform))
            {
                Debug.Log($"[CardRender] Skipping card's own collider: {collider.name}");
                continue;
            }

            Debug.Log($"[CardRender] Hit: {collider.name}, layer: {LayerMask.LayerToName(collider.gameObject.layer)}");

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

    private bool CheckForEndTurnEffect()
    {
        if (Data == null) return false;

        if (Data.effects != null)
        {
            foreach (var effect in Data.effects)
                if (effect.operationType == OperationType.EndTurn)
                    return true;
        }

        if (Instance != null && Instance.rolledEffects != null)
        {
            foreach (var effect in Instance.rolledEffects)
                if (effect.operationType == OperationType.EndTurn)
                    return true;
        }

        return false;
    }

    private void OnMouseOver()
    {
        if (_fxHelper != null && !_isDragging)
            _fxHelper.OnCardHover(this);
    }

    private void OnMouseExit()
    {
        // No-op; exit visuals are handled elsewhere
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

    // Performance: Optimized array search helper
    private T FindInArray<T>(T[] array, string contains) where T : Component
    {
        string search = contains.ToLowerInvariant();
        foreach (var item in array)
        {
            if (item.name.ToLowerInvariant().Contains(search))
                return item;
        }
        return null;
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
