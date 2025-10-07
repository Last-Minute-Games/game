using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class JournalUI : MonoBehaviour
{
    [Header("Refs")]
    public Button journalButton;         // hook your JournalButton here
    public CanvasGroup journalPanel;     // the content panel that appears when open

    [Header("UI Behavior")]
    public float fadeDuration = 0.15f;   // fade for the contents

    Animator anim;
    bool isOpen;

    void Awake()
    {
        anim = GetComponent<Animator>();
        Debug.Log("[JournalUI] Awake called.");

        if (journalButton)
        {
            journalButton.onClick.AddListener(Toggle);
            Debug.Log("[JournalUI] Journal button listener attached.");
        }
        else
        {
            Debug.LogWarning("[JournalUI] Journal button reference not assigned!");
        }

        SetOpen(false, instant: true);
    }

    public void Toggle()
    {
        Debug.Log($"[JournalUI] Toggle pressed. Current state: {(isOpen ? "Open" : "Closed")}");
        SetOpen(!isOpen);
    }

    public void Open()
    {
        Debug.Log("[JournalUI] Open() called.");
        SetOpen(true);
    }

    public void Close()
    {
        Debug.Log("[JournalUI] Close() called.");
        SetOpen(false);
    }

    void SetOpen(bool value, bool instant = false)
    {
        Debug.Log($"[JournalUI] SetOpen called. Target state: {(value ? "Open" : "Closed")}, Instant: {instant}");

        isOpen = value;
        if (anim != null)
        {
            anim.SetBool("Open", isOpen);
            Debug.Log($"[JournalUI] Animator parameter 'Open' set to {isOpen}");
        }
        else
        {
            Debug.LogError("[JournalUI] Animator reference missing!");
        }

        if (!journalPanel)
        {
            Debug.LogWarning("[JournalUI] No journalPanel assigned, skipping fade.");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(FadePanel(isOpen, instant ? 0f : fadeDuration));
    }

    IEnumerator FadePanel(bool show, float dur)
    {
        Debug.Log($"[JournalUI] FadePanel started. Show: {show}, Duration: {dur}");

        journalPanel.blocksRaycasts = show;
        journalPanel.interactable = show;

        float start = journalPanel.alpha;
        float end = show ? 1f : 0f;
        float t = 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float progress = dur <= 0 ? 1f : t / dur;
            journalPanel.alpha = Mathf.Lerp(start, end, progress);

            Debug.Log($"[JournalUI] Fading... progress={progress:F2}, alpha={journalPanel.alpha:F2}");
            yield return null;
        }

        journalPanel.alpha = end;
        Debug.Log($"[JournalUI] Fade complete. Final alpha={journalPanel.alpha:F2}");
    }
}
