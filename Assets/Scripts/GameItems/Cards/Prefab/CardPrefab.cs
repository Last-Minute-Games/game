using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardPrefab : MonoBehaviour
{
    [Header("Card Data Reference")]
    public CardData cardData;

    [Header("UI References")]
    public Image cardBackground;
    public Image cardIcon;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text intentionText;
    public TMP_Text energyCost;


    private void Awake()
    {
        if (cardData == null)
            Debug.LogWarning($"[CardPrefab] '{name}' has no CardData assigned before Start — will wait for CardManager to initialize.");
    }

    private void Start()
    {
        if (cardData != null)
            Initialize(cardData);
    }

    public void Initialize(CardData data)
    {
        if (data == null)
        {
            Debug.LogError("[CardPrefab] Tried to initialize with null CardData!");
            return;
        }

        cardData = data;

        // --- Set visuals ---
        nameText?.SetText(data.itemName);
        descriptionText?.SetText(data.description);
        intentionText?.SetText(data.intentionText);
        energyCost?.SetText(data.energyCost.ToString());

        if (cardBackground)
            cardBackground.sprite = data.artwork;

        if (cardIcon)
            cardIcon.sprite = data.icon;

        Debug.Log($"[CardPrefab] Initialized card: {data.itemName}");
    }

    public void PlayCard()
    {
        // Placeholder for later: CardManager will handle logic & FX
        Debug.Log($"[CardPrefab] Card '{cardData?.itemName ?? "Unnamed"}' played.");
    }
}
