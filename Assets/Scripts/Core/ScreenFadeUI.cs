using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

public class ScreenFadeUI : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI resultText;

    [Header("Settings")]
    public float fadeDuration = 1.0f;
    
    [Header("Round Transition Settings")]
    public float roundTransitionFadeInTime = 0.5f;
    public float roundTransitionHoldTime = 1.0f;
    public float roundTransitionFadeOutTime = 0.5f;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowMessage(string message, Color color)
    {
        resultText.text = message;
        resultText.color = color;

        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, fadeDuration);
    }
    
    public void HideMessage()
    {
        canvasGroup.DOFade(0f, fadeDuration);
    }

    /// <summary>
    /// Shows a wave transition (e.g., "WAVE 1", "WAVE 2") with fade in/out effect
    /// </summary>
    public IEnumerator ShowRoundTransition(int waveNumber, bool isNewWave = false)
    {
        string message = $"WAVE {waveNumber}";
        Color color = new Color(1f, 0.5f, 0f); // Orange for waves
        
        resultText.text = message;
        resultText.color = color;

        // Fade in immediately (no delay)
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, roundTransitionFadeInTime);
        yield return new WaitForSeconds(roundTransitionFadeInTime);

        // Hold briefly
        yield return new WaitForSeconds(roundTransitionHoldTime);

        // Fade out as the wave loads
        canvasGroup.DOFade(0f, roundTransitionFadeOutTime);
        yield return new WaitForSeconds(roundTransitionFadeOutTime);
    }
}
