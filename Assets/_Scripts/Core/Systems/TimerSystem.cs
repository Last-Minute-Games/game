using UnityEngine;
using TMPro;

public class TurnTimer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text timerText;

    [Header("Settings")]
    [SerializeField] private float turnDuration = 20f; // seconds
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField, Range(0f, 1f)] private float warningThreshold = 0.25f;

    private float timer;
    private bool isRunning = false;
    private bool hasEndedTurn = false; // ✅ prevents double-end calls
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

        // 🔹 Update text
        if (timerText)
        {
            timerText.text = $"{Mathf.CeilToInt(timer)}";

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

        // ⏰ Expired
        if (timer <= 0f && !hasEndedTurn)
        {
            hasEndedTurn = true;
            ForceEndTurn();
        }
    }

    // ────────────────────────────────
    // Public Control
    // ────────────────────────────────
    public void StartTimer()
    {
        ResetTimer();
        isRunning = true;
        hasEndedTurn = false;
        Debug.Log("▶ Timer started.");
    }

    public void StopTimer()
    {
        isRunning = false;
        Debug.Log("⏹ Timer stopped.");
    }

    public void PauseTimer()
    {
        isRunning = false;
        Debug.Log("⏸ Timer paused.");
    }

    public void ResumeTimer()
    {
        isRunning = true;
        Debug.Log("▶ Timer resumed.");
    }

    public void ResetWithoutStart()
    {
        timer = turnDuration;
        hasEndedTurn = false;
        if (timerText)
        {
            timerText.text = $"{Mathf.CeilToInt(timer)}";
            timerText.color = normalColor;
        }
        Debug.Log("🔄 Timer reset (no auto-start).");
    }

    public void ResetTimer()
    {
        timer = turnDuration;
        hasEndedTurn = false;
        if (timerText)
        {
            timerText.text = $"{Mathf.CeilToInt(timer)}";
            timerText.color = normalColor;
        }
    }

    // ────────────────────────────────
    // Internal
    // ────────────────────────────────
    private void ForceEndTurn()
    {
        if (!isRunning) return;

        isRunning = false;
        Debug.Log("⏰ Timer expired — requesting turn end.");

        if (battleSystem != null)
        {
            // Call via coroutine-safe wrapper to ensure it triggers
            battleSystem.RequestTurnEndFromTimer();
        }
        else
        {
            Debug.LogWarning("⚠️ No BattleSystem found for timer end.");
        }
    }
}
