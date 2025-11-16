using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
public class CardRender : MonoBehaviour
{
    [Header("UI References")] 
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image cardIcon;
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

    private void Awake()
    {
        // Auto-wire references if not assigned
        if (cardBackground == null) cardBackground = FindChildByName<Image>("CardBackground");
        if (cardIcon == null) cardIcon = FindChildByName<Image>("CardIcon");
        if (energyCost == null) energyCost = FindChildByName<TMP_Text>("EnergyCost");
        if (cardName == null) cardName = FindChildByName<TMP_Text>("CardName");
        if (descriptionText == null) descriptionText = FindChildByName<TMP_Text>("DescriptionText");
    }

    public void Bind(CardData data, int? energy = null)
    {
        Data = data;
        // Name/Description
        if (cardName != null) cardName.text = data != null ? data.name : string.Empty;
        if (descriptionText != null) descriptionText.text = data != null ? data.description : string.Empty;

        // Icon
        if (cardIcon != null)
        {
            var sprite = data != null && data.artwork != null ? data.artwork : fallbackIcon;
            cardIcon.sprite = sprite;
            cardIcon.enabled = sprite != null;
            // Preserve aspect for nicer visuals
            cardIcon.preserveAspect = true;
        }

        // Energy
        int energyVal = energy ?? defaultEnergyCost;
        if (energyCost != null)
        {
            energyCost.text = energyVal > 0 ? energyVal.ToString() : string.Empty;
        }
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
