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

    void Awake()
    {
        anim = GetComponent<Animator>();
        cg = journalPanel.transform.GetComponent<CanvasGroup>();

        // Start closed
        SetOpen(false, instant: true);

        // Wire automatically if provided
        if (toggleButton) toggleButton.onClick.AddListener(Toggle);
    }

    public void Toggle() => SetOpen(!isOpen, true);

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
            if (instant) cg.alpha = open ? 1f : 0f; // for fade-style animations; harmless if using flipbook only
        }
    }
}
