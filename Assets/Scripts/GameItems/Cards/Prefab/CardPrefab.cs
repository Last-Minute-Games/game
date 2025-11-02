using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardPrefab : MonoBehaviour
{
    [Header("Card Data Reference")]
    public CardData cardData;

    [Header("UI References")]
    public Image artworkImage;
    public Image intentionIcon;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text intentionText;
    public TMP_Text energyCost;

    [Header("Helpers")]
    public CardFXHelper fxHelper;

    private void Awake()
    {
        if (fxHelper == null)
            fxHelper = GetComponent<CardFXHelper>();
    }

    private void Start()
    {
        if (cardData != null)
            Initialize(cardData);
    }

    public void Initialize(CardData data)
    {
        cardData = data;

        // --- Set visuals ---
        nameText.text = data.itemName;
        descriptionText.text = data.description;
        intentionText.text = data.intentionText;
        energyCost.text = data.energyCost;
        artworkImage.sprite = data.artwork;
        intentionIcon.sprite = data.icon;

        // --- Initialize FX helpers ---
        fxHelper?.Initialize(data);
    }

    public void PlayCard()
    {
        fxHelper?.PlayCardFX();
        // send data to CardManager for logic processing later
    }
}
