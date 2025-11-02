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
        { // D.V. for fxHelper
            Debug.Log($"CardData {cardData?.itemName ?? "Unnamed"} did not have a CardFXHelper component. Pulling manually.");
            fxHelper = GetComponent<CardFXHelper>();
        }
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
        }

        cardData = data;

        // --- Set visuals ---
        nameText.SetText(data.itemName);
        descriptionText.SetText(data.description);
        intentionText.SetText(data.intentionText);
        energyCost.SetText(data.energyCost.ToString());
        if (artworkImage) artworkImage.sprite = data.artwork;
        if (intentionIcon) intentionIcon.sprite = data.icon;

        // --- Initialize FX helpers ---
        fxHelper?.Initialize(data);
    }

    public void PlayCard()
    {
        fxHelper?.PlayCardFX();
        // send data to CardManager for logic processing later
    }
}
