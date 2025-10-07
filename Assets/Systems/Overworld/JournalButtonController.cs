using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator), typeof(CanvasGroup))]
public class JournalUI : MonoBehaviour
{
    [SerializeField] Button toggleButton;   // Drag your JournalButton here (optional if wiring via inspector)
    [SerializeField] GameObject journalPanel;

    Animator anim;
    CanvasGroup cg;
    bool isOpen;

    public float fadeDuration = 0.2f;      // How long the fade takes

    IEnumerator FadeToAlpha(float endAlpha, float duration)
    {
        float startAlpha = cg.alpha;
        float time = 0f;

        while (time < duration)
        {
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            time += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        cg.alpha = endAlpha; // Ensure it ends exactly at the target
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        cg = journalPanel.transform.GetComponent<CanvasGroup>();

        // Start closed
        SetOpen(false, instant: true);

        // Wire automatically if provided
        if (toggleButton) toggleButton.onClick.AddListener(Toggle);
    }

    public void Toggle() => SetOpen(!isOpen);

    public void Open() => SetOpen(true);
    public void Close() => SetOpen(false);

    void SetOpen(bool open, bool instant = false)
    {
        Debug.Log($"JournalUI SetOpen({open})");

        isOpen = open;

        if (anim) anim.SetBool("Open", open);

        // Make it non-clickable when closed so it doesn't block other UI
        if (cg)
        {
            cg.blocksRaycasts = open;
            cg.interactable = open;

            float targetAlpha = open ? 1f : 0f;

            if (instant)
            {
                cg.alpha = targetAlpha;
            }
            else
            {
                StartCoroutine(FadeToAlpha(targetAlpha, fadeDuration));
            }
            ; // for fade-style animations; harmless if using flipbook only
        }
    }
}
