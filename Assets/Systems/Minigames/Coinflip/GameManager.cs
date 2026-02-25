using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public FlipScript coin; // <-- assign the UI Image coin (with FlipScript)

    [Header("UI")]
    public Button headsButton;
    public Button tailsButton;
    public Button endTurnButton;
    public Button autoPlayButton;
    public Button resetButton;

    public TMP_Text playerScoreText;
    public TMP_Text aiScoreText;
    public TMP_Text roundText;
    public TMP_Text statusText;

    [Header("Rules")]
    public int targetScore = 5;

    // Internal state
    private const int HEADS = 0;
    private const int TAILS = 1;

    private int playerChoice = -1; // -1 until chosen
    private int playerScore = 0;
    private int aiScore = 0;
    private int roundIndex = 1;
    private bool isFlipping = false;
    private bool gameOver = false;
    private bool autoMode = false;

    // Track if the player has completed at least one full match (player or AI reaching targetScore)
    private bool hasCompletedMatch = false;

    // Public read-only access so other scripts can query state
    public bool GameOver => gameOver;
    public bool HasCompletedMatch => hasCompletedMatch;

    public bool PlayerWonMatch => gameOver && playerScore > aiScore;
    public bool AiWonMatch => gameOver && aiScore > playerScore;


    void Start()
    {
        HookButtons();
        UpdateUI();
        statusText.text = "Choose Heads or Tails.";
    }

    private void HookButtons()
    {
        headsButton.onClick.RemoveAllListeners();
        tailsButton.onClick.RemoveAllListeners();

        headsButton.onClick.AddListener(() => Choose(HEADS) && if(!isFlipping && !gameOver) StartCoroutine(DoRound()));
        tailsButton.onClick.AddListener(() => Choose(TAILS) && if(!isFlipping && !gameOver) StartCoroutine(DoRound()));
        
        
    }

    private void Choose(int choice)
    {
        if (isFlipping || gameOver) return;

        playerChoice = choice;
        statusText.text = (choice == HEADS)
            ? "You chose HEADS. "
            : "You chose TAILS. ";

        UpdateUI();
    }

    private IEnumerator DoRound()
    {
        if (playerChoice == -1)
        {
            statusText.text = "Pick Heads or Tails first.";
            yield break;
        }

        isFlipping = true;
        statusText.text = "Flipping...";
        int result = -1;

        // Trigger the flip. We pass a random outcome request (true=heads, false=tails)
        bool forceHeads = (Random.value < 0.5f);
        coin.Flip(forceHeads, r => result = r);

        // Wait until FlipScript calls back with the result
        while (result == -1)
            yield return null;

        // AI picks opposite of player for a guaranteed single point each round
        int aiChoice = (playerChoice == HEADS) ? TAILS : HEADS;

        // Score
        if (result == playerChoice) playerScore++;
        else aiScore++;

        // Round summary
        string resText = (result == HEADS) ? "HEADS" : "TAILS";
        string pText = (playerChoice == HEADS) ? "HEADS" : "TAILS";
        string aText = (aiChoice == HEADS) ? "HEADS" : "TAILS";
        statusText.text = $"Result: {resText}. You picked {pText}, AI picked {aText}.";

        roundIndex++;
        playerChoice = -1; // require a fresh choice each round
        UpdateUI();

        // Win check
        if (playerScore >= targetScore )
        {
            gameOver = true;
            hasCompletedMatch = true;
            statusText.text = "Match over! You win the match!";
           
        }

        isFlipping = false;
    }

    private void UpdateUI()
    {
        if (playerScoreText) playerScoreText.text = $"You: {playerScore}";
        if (roundText) roundText.text = $"Round {roundIndex}";
        if (endTurnButton) endTurnButton.interactable = (playerChoice != -1) && !isFlipping && !gameOver;
    }

}
