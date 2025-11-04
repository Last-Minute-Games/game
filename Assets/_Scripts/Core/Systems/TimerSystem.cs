using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TurnTimer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text timerText; // ⬅️ only using the text now

    [Header("Settings")]
    [SerializeField] private float turnDuration = 20f; // seconds
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningThreshold = 0.25f;

    private float timer;
    private bool isRunning = false;
    private BattleSystem battleSystem;

    private void Awake()
    {
        battleSystem = FindFirstObjectByType<BattleSystem>();
        ResetTimer();
    }

    private void Update()
    {
        if (!isRunning) return;

        timer -= Time.deltaTime;
        float progress = Mathf.Clamp01(timer / turnDuration);

        // 🕒 Update text
        if (timerText)
        {
            timerText.text = $"{Mathf.CeilToInt(timer)}";

            // Change color if near end
            timerText.color = (progress < warningThreshold)
                ? warningColor
                : normalColor;
        }

        if (timer <= 0f)
            ForceEndTurn();

        if (progress < warningThreshold)
        {
            float pulse = Mathf.Abs(Mathf.Sin(Time.time * 5f)) * 0.5f + 0.5f;
            timerText.color = Color.Lerp(normalColor, warningColor, pulse);
        }
        else
        {
            timerText.color = normalColor;
        }
    }

    public void StartTimer()
    {
        ResetTimer();
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        timer = turnDuration;
        if (timerText)
        {
            timerText.text = $"{Mathf.CeilToInt(timer)}";
            timerText.color = normalColor;
        }
    }

    private void ForceEndTurn()
    {
        isRunning = false;
        if (battleSystem != null)
        {
            Debug.Log("⏰ Timer expired — ending player turn automatically.");
            battleSystem.EndPlayerTurn();
        }
    }
}
