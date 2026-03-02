using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Shared black-screen transition used by Sokoban, Coinflip, Blackjack, and Maze.
/// Fade to black -> run midAction -> wait -> fade out. Optional afterTransition runs at the end.
/// </summary>
public class MinigameTransition : MonoBehaviour
{
    [Header("Transition")]
    [Tooltip("CanvasGroup used to fade the screen when entering/exiting the minigame.")]
    [SerializeField] CanvasGroup transitionCanvasGroup;
    [Tooltip("Optional text element that displays the current transition message.")]
    [SerializeField] TMP_Text transitionStatusText;
    [Tooltip("How long (in seconds) the screen stays fully faded while we reposition objects.")]
    [SerializeField] float transitionCoveredDuration = 1.2f;
    [Tooltip("Fade-in duration in seconds.")]
    [SerializeField] float transitionFadeInDuration = 0.4f;
    [Tooltip("Fade-out duration in seconds.")]
    [SerializeField] float transitionFadeOutDuration = 0.4f;

    private Coroutine _routine;
    private bool _isRunning;

    public bool IsTransitionRunning => _isRunning;

    void Awake()
    {
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.blocksRaycasts = false;
            transitionCanvasGroup.interactable = false;
            transitionCanvasGroup.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Run the transition: fade to black -> midAction -> wait -> fade out -> afterTransition.
    /// </summary>
    /// <param name="message">Shown on transitionStatusText if set.</param>
    /// <param name="midAction">Runs while the screen is fully black.</param>
    /// <param name="coveredDurationOverride">If set, overrides transitionCoveredDuration for this run.</param>
    /// <param name="afterTransition">Runs after the fade-out (e.g. hide popup root).</param>
    /// <param name="instantBlack">If true, snap to full black immediately (no fade-in), then run midAction, wait, fade out. Use for exit so main game never peeks through.</param>
    public void RunTransition(string message, Action midAction, float? coveredDurationOverride = null, Action afterTransition = null, bool instantBlack = false)
    {
        if (_isRunning) return;

        if (transitionCanvasGroup == null)
        {
            midAction?.Invoke();
            afterTransition?.Invoke();
            return;
        }

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        float covered = coveredDurationOverride ?? transitionCoveredDuration;
        _routine = StartCoroutine(TransitionRoutine(message, midAction, covered, afterTransition, instantBlack));
    }

    private IEnumerator TransitionRoutine(string message, Action midAction, float coveredDuration, Action afterTransition, bool instantBlack)
    {
        _isRunning = true;

        if (transitionStatusText != null)
            transitionStatusText.text = message;

        GameObject go = transitionCanvasGroup.gameObject;
        if (!go.activeSelf) go.SetActive(true);
        transitionCanvasGroup.blocksRaycasts = true;
        transitionCanvasGroup.interactable = true;

        // Ensure overlay is drawn on top of minigame/popup so we fade to black over the minigame, not behind it
        Canvas canvas = transitionCanvasGroup.GetComponentInParent<Canvas>();
        if (canvas != null)
            canvas.sortingOrder = 999;

        if (instantBlack)
        {
            transitionCanvasGroup.alpha = 1f;
        }
        else
        {
            yield return FadeCanvasGroup(transitionCanvasGroup.alpha, 1f, transitionFadeInDuration);
        }

        midAction?.Invoke();

        if (coveredDuration > 0f)
            yield return new WaitForSeconds(coveredDuration);

        yield return FadeCanvasGroup(transitionCanvasGroup.alpha, 0f, transitionFadeOutDuration);

        transitionCanvasGroup.blocksRaycasts = false;
        transitionCanvasGroup.interactable = false;
        go.SetActive(false);

        _routine = null;
        _isRunning = false;

        afterTransition?.Invoke();
    }

    private IEnumerator FadeCanvasGroup(float start, float end, float duration)
    {
        if (duration <= 0f)
        {
            transitionCanvasGroup.alpha = end;
            yield break;
        }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transitionCanvasGroup.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }
        transitionCanvasGroup.alpha = end;
    }
}
