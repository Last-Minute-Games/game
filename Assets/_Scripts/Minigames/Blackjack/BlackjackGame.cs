using System.Collections;
using System.Collections.Generic;
using Blackjack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BlackjackGame : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text dealerHandText;
    public TMP_Text playerHandText;
    public TMP_Text statusText;
    public Button hitButton;
    public Button standButton;
    public Button newGameButton;


    [Header("Options")]
    [Tooltip("Number of 52-card decks to combine.")]
    public int numberOfDecks = 1;


    private Deck deck;
    private Hand dealer = new Hand();
    private Hand player = new Hand();
    private bool playerTurn;


    [Header("Card UI")]
    public Transform dealerHandArea;
    public Transform playerHandArea;
    public CardViews cardPrefab;   // the prefab from step 3
    public CardSpriteLibrary spriteLibrary;

    readonly List<GameObject> temp = new List<GameObject>(); //optional???



    void Awake()
    {
        deck = new Deck(numberOfDecks);
        WireButtons();
    }


    void Start()
    {
        StartNewRound();
    }


    void WireButtons()
    {
        hitButton.onClick.AddListener(OnHit);
        standButton.onClick.AddListener(OnStand);
        newGameButton.onClick.AddListener(StartNewRound);
    }

    public void StartNewRound()
    {
        dealer.Clear();
        player.Clear();
        statusText.text = "";


        if (deck.Count < 15) deck.Rebuild(numberOfDecks);


        // Initial deal (player, dealer, player, dealer)
        player.Add(deck.Draw());
        dealer.Add(deck.Draw());
        player.Add(deck.Draw());
        dealer.Add(deck.Draw());


        playerTurn = true;
        UpdateUI(hideDealerHoleCard: true);


        // Check naturals
        if (player.IsBlackjack() || dealer.IsBlackjack())
        {
            ResolveRound(finalReveal: true);
        }
        else
        {
            SetButtonsState(hit: true, stand: true);
        }
    }

    public void OnHit()
    {
        if (!playerTurn) return;
        player.Add(deck.Draw());
        UpdateUI(hideDealerHoleCard: true);
        if (player.IsBust())
        {
            statusText.text = "Player busts! Dealer wins.";
            EndPlayerTurn();
        }
    }

    public void OnStand()
    {
        if (!playerTurn) return;
        EndPlayerTurn();
    }

    void EndPlayerTurn()
    {
        playerTurn = false;
        SetButtonsState(hit: false, stand: false);
        StartCoroutine(DealerPlay());
    }

    IEnumerator DealerPlay()
    {
        // Dealer reveals hole card and hits to 17 (stand on soft 17 configurable if you want)
        UpdateUI(hideDealerHoleCard: false);
        yield return new WaitForSeconds(0.4f);


        while (dealer.Total() < 17)
        {
            dealer.Add(deck.Draw());
            UpdateUI(hideDealerHoleCard: false);
            yield return new WaitForSeconds(0.35f);
        }


        ResolveRound(finalReveal: true);
    }

    void ResolveRound(bool finalReveal)
    {
        if (finalReveal) UpdateUI(hideDealerHoleCard: false);


        var p = player.Total();
        var d = dealer.Total();


        if (player.IsBlackjack() && dealer.IsBlackjack())
            statusText.text = "Push: both blackjack.";
        else if (player.IsBlackjack())
            statusText.text = "Blackjack! Player wins.";
        else if (dealer.IsBlackjack())
            statusText.text = "Dealer blackjack. Dealer wins.";
        else if (player.IsBust())
            statusText.text = "Player busts! Dealer wins.";
        else if (dealer.IsBust())
            statusText.text = "Dealer busts! Player wins.";
        else if (p > d)
            statusText.text = "Player wins.";
        else if (p < d)
            statusText.text = "Dealer wins.";
        else
            statusText.text = "Push.";


        SetButtonsState(hit: false, stand: false);
    }

    void UpdateUI(bool hideDealerHoleCard)
    {
        /*
        playerHandText.text = FormatHand(player, revealAll: true);
        dealerHandText.text = hideDealerHoleCard
            ? FormatDealerWithHoleCardHidden()
            : FormatHand(dealer, revealAll: true);
        */
        playerHandText.text = $"Total: {player.Total()}";

        if (hideDealerHoleCard)
        {
            // Show the dealer's first card face + '?'
            if (dealer.Cards.Count > 0)
            {
                var firstCard = dealer.Cards[0];
                dealerHandText.text = $"{firstCard.Face} + ?";
            }
            else
            {
                dealerHandText.text = "Total: ?";
            }
        }
        else
        {
            // Show full total once revealed
            dealerHandText.text = $"Total: {dealer.Total()}";
        }
        // Player: all face-up
        RenderHandSprites(playerHandArea, player.Cards, revealAll: true, hideHoleCard: false);

        // Dealer: show all, but flip the hole card if requested
        RenderHandSprites(dealerHandArea, dealer.Cards, revealAll: true, hideHoleCard: hideDealerHoleCard);
    }


    string FormatHand(Hand h, bool revealAll)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < h.Cards.Count; i++)
        {
            var c = h.Cards[i];
            sb.Append(c.Face).Append("\u2009").Append(SuitGlyph(c.Suit));
            if (i < h.Cards.Count - 1) sb.Append(", ");
        }
        sb.Append($"\nTotal: {h.Total()}");
        return sb.ToString();
    }

    string FormatDealerWithHoleCardHidden()
    {
        if (dealer.Cards.Count == 0) return "";
        var shown = dealer.Cards[0];
        int visible = shown.Value == 11 ? 11 : shown.Value;
        return $"{shown.Face}\u2009{SuitGlyph(shown.Suit)}, [??]\\nTotal: {visible}+?";
    }


    void SetButtonsState(bool hit, bool stand)
    {
        hitButton.interactable = hit;
        standButton.interactable = stand;
    }

    static string SuitGlyph(Suit s)
    {
        switch (s)
        {
            case Suit.Clubs: return "♣";
            case Suit.Diamonds: return "♦";
            case Suit.Hearts: return "♥";
            default: return "♠";
        }
    }
    public System.Action OnRequestClose;

    public void QuitToOverworld() { 
        OnRequestClose?.Invoke();
    }

    void ClearArea(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
            Destroy(t.GetChild(i).gameObject);
    }

    void RenderHandSprites(Transform area, List<Blackjack.Card> cards, bool revealAll, bool hideHoleCard)
    {
        ClearArea(area);
        for (int i = 0; i < cards.Count; i++)
        {
            var cv = Instantiate(cardPrefab, area);
            cv.library = spriteLibrary; // ensure set
            bool faceUp = revealAll && !(hideHoleCard && i == 1);
            cv.Show(cards[i], faceUp);
        }
    }




}


    