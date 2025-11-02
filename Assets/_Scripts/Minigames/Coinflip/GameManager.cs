using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public FlipScript coin;

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
    const int HEADS = 0;
    const int TAILS = 1;

    int playerChoice = -1; // -1 until chosen
    int playerScore = 0;
    int aiScore = 0;
    int roundIndex = 1;
    bool isFlipping = false;
    bool gameOver = false;
    bool autoMode = false;

    void Start()
    {
        HookButtons();
        UpdateUI();
        statusText.text = "Choose Heads or Tails.";
    }

    void HookButtons()
    {
        headsButton.onClick.AddListener(() => Choose(HEADS));
        tailsButton.onClick.AddListener(() => Choose(TAILS));
        endTurnButton.onClick.AddListener(() => { if (!isFlipping && !gameOver) StartCoroutine(DoRound()); });
        autoPlayButton.onClick.AddListener(() => { if (!gameOver) StartCoroutine(AutoPlayToWin()); });
        resetButton.onClick.AddListener(ResetMatch);
    }

    void Choose(int choice)
    {
        if (isFlipping || gameOver) return;
        playerChoice = choice;
        statusText.text = (choice == HEADS) ? "You chose HEADS. End Turn to flip!" : "You chose TAILS. End Turn to flip!";
        UpdateUI();
    }

    IEnumerator DoRound()
    {
        if (playerChoice == -1)
        {
            statusText.text = "Pick Heads or Tails first.";
            yield break;
        }

        isFlipping = true;
        statusText.text = "Flipping...";
        int result = -1;

        // Flip coin and wait for animation/result
        yield return StartCoroutine(coin.Flip(0.01f, 0.07f, r => result = r));

        // AI picks opposite of player to guarantee a single point per round
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
            statusText.text += (playerScore > aiScore) ? " You win the match! 🎉" : " AI wins the match! 🤖";
        }

        isFlipping = false;
    }

    IEnumerator AutoPlayToWin()
    {
        if (isFlipping || gameOver) yield break;
        autoMode = true;
        statusText.text = "Autoplay: playing rounds until someone reaches the target.";
        while (!gameOver)
        {
            // Optional: lock player to HEADS for speed; or randomize:
            playerChoice = (Random.value < 0.5f) ? HEADS : TAILS;
            yield return StartCoroutine(DoRound());
            yield return new WaitForSeconds(0.25f);
        }
        autoMode = false;
    }

    void UpdateUI()
    {
        playerScoreText.text = $"You: {playerScore}";
        aiScoreText.text = $"AI: {aiScore}";
        roundText.text = $"Round {roundIndex}";
        endTurnButton.interactable = (playerChoice != -1) && !isFlipping && !gameOver;
    }

    void ResetMatch()
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
