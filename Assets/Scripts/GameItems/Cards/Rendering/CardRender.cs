using GameItems.Cards;
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
        
        _fxHelper = GetComponent<CardFXHelper>();
        if (_fxHelper == null) _fxHelper = gameObject.AddComponent<CardFXHelper>();
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
            energyCost.text = energyVal > 0 ? energyVal.ToString() : string.Empty;
        }
    }

    public void Bind(CardInstance instance, int? energy = null)
    {
        Instance = instance;
        Data = instance != null ? instance.data : null;

        // Name with variability tier prefix if present
        if (cardName != null)
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
            string baseDesc = Data != null ? Data.description : string.Empty;
            if (instance != null && instance.rolledEffects != null && instance.rolledEffects.Count > 0)
            {
                int dmg = instance.GetTotal(OperationType.Damage);
                int blk = instance.GetTotal(OperationType.AddShield);
                string summary = string.Empty;
                if (dmg != 0) summary += $" +{dmg} Damage";
                if (blk != 0) summary += (summary.Length > 0 ? "," : "") + $" +{blk} Block";
                if (!string.IsNullOrEmpty(summary)) baseDesc = $"{baseDesc}\n[{summary.Trim()}]";
            }
            descriptionText.text = baseDesc;
        }

        // Sprites from data
        cardBackground.sprite = Data != null ? Data.artwork : null;
        cardIcon.sprite = Data != null ? Data.icon : null;

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
        // Intentionally no FX call on exit to avoid snapping animations;
        // CardAnimationHelper will restore on release/cancel when appropriate.
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_fxHelper != null)
        {
            _fxHelper.OnCardSelect(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        if (_fxHelper != null)
        {
            // Ensure select visuals/sfx when drag starts
            _fxHelper.OnCardSelect(this);
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
            // Without validation context here, default to invalid target
            _fxHelper.OnCardRelease(this, validTarget: false);
        }
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

