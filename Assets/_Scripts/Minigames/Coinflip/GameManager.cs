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
        endTurnButton.onClick.RemoveAllListeners();
        autoPlayButton.onClick.RemoveAllListeners();
        resetButton.onClick.RemoveAllListeners();

        headsButton.onClick.AddListener(() => Choose(HEADS));
        tailsButton.onClick.AddListener(() => Choose(TAILS));
        endTurnButton.onClick.AddListener(() =>
        {
            if (!isFlipping && !gameOver) StartCoroutine(DoRound());
        });
        autoPlayButton.onClick.AddListener(() =>
        {
            if (!gameOver) StartCoroutine(AutoPlayToWin());
        });
        resetButton.onClick.AddListener(ResetMatch);
    }

    private void Choose(int choice)
    {
        if (isFlipping || gameOver) return;

        playerChoice = choice;
        statusText.text = (choice == HEADS)
            ? "You chose HEADS. End Turn to flip!"
            : "You chose TAILS. End Turn to flip!";

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
        if (playerScore >= targetScore || aiScore >= targetScore)
        {
            gameOver = true;
            statusText.text += (playerScore > aiScore)
                ? " You win the match! 🎉"
                : " AI wins the match! 🤖";
        }

        isFlipping = false;
    }

    private IEnumerator AutoPlayToWin()
    {
        if (isFlipping || gameOver) yield break;

        autoMode = true;
        statusText.text = "Autoplay: playing rounds until someone reaches the target.";
        while (!gameOver)
        {
            // Random choice each round for the player
            playerChoice = (Random.value < 0.5f) ? HEADS : TAILS;

            yield return StartCoroutine(DoRound());

            // Small pacing delay between rounds
            yield return new WaitForSeconds(0.25f);
        }

        autoMode = false;
    }

    private void UpdateUI()
    {
        if (playerScoreText) playerScoreText.text = $"You: {playerScore}";
        if (aiScoreText) aiScoreText.text = $"AI: {aiScore}";
        if (roundText) roundText.text = $"Round {roundIndex}";
        if (endTurnButton) endTurnButton.interactable = (playerChoice != -1) && !isFlipping && !gameOver;
    }

    private void ResetMatch()
    {
        StopAllCoroutines();
        playerScore = 0;
        aiScore = 0;
        roundIndex = 1;
        playerChoice = -1;
        gameOver = false;
        isFlipping = false;
        autoMode = false;

        statusText.text = "New match. Choose Heads or Tails.";
        UpdateUI();
    }
}
