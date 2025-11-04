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

    [Header("Match / Score")]
    public int targetWins = 5;
    public TMP_Text scoreText;   // drag a UI text here: “Player 0 – 0 Dealer”

    [Header("Match End")]
    public float endMatchCloseDelay = 1.25f;

    private int playerWins = 0;
    private int dealerWins = 0;
    private bool matchOver = false;

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

        UpdateScoreUI();
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

        int p = player.Total();
        int d = dealer.Total();

        string outcome;
        if (player.IsBust()) outcome = "Player busts. Dealer wins.";
        else if (d > 21) outcome = "Dealer busts. Player wins.";
        else if (player.IsBlackjack() && !dealer.IsBlackjack()) outcome = "Blackjack! Player wins.";
        else if (dealer.IsBlackjack() && !player.IsBlackjack()) outcome = "Dealer blackjack. Dealer wins.";
        else if (p > d) outcome = "Player wins.";
        else if (p < d) outcome = "Dealer wins.";
        else outcome = "Push.";

        statusText.text = outcome;

        // --- match scoring ---
        if (!matchOver)
        {
            if (outcome.Contains("Player wins")) playerWins++;
            else if (outcome.Contains("Dealer wins")) dealerWins++;
            // Push: no points

            UpdateScoreUI();

            if (playerWins >= targetWins || dealerWins >= targetWins)
            {
                matchOver = true;
                statusText.text += $"\n\nMatch over — {(playerWins > dealerWins ? "Player" : "Dealer")} reaches {targetWins}.";
                // lock actions until New/Reset
                hitButton.interactable = false;
                standButton.interactable = false;
            }
        }

        if (playerWins >= targetWins || dealerWins >= targetWins)
        {
            matchOver = true;
            statusText.text += $"\n\nMatch over — {(playerWins > dealerWins ? "Player" : "Dealer")} reaches {targetWins}.";
            hitButton.interactable = false;
            standButton.interactable = false;

            // NEW: close after a short delay
            StartCoroutine(CloseAfterDelay(endMatchCloseDelay));
        }

        // disable round buttons; player must click New to deal next round
        hitButton.interactable = false;
        standButton.interactable = false;
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

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Player {playerWins}  —  {dealerWins} Dealer";
    }

    IEnumerator CloseAfterDelay(float s)
    {
        yield return new WaitForSeconds(s);
        OnRequestClose?.Invoke();   // popup will Hide()
    }







}


