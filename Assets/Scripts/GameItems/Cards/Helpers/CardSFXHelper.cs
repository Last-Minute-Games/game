using UnityEngine;

public class CardSFXHelper : MonoBehaviour
{
    [Header("Card Action Sounds")]
    public SFXCueData drawCue;
    public SFXCueData hoverCue;
    public SFXCueData selectCue;
    public SFXCueData dragCue;
    public SFXCueData confirmCue;
    public SFXCueData cancelCue;
    public SFXCueData discardCue;

    // ────────────────────────────────
    // Playback API
    // ────────────────────────────────

    public void PlayDraw()
    {
        SFXManager.Instance?.Play(drawCue);
    }

    public void PlayHover()
    {
        SFXManager.Instance?.Play(hoverCue);
    }

    public void PlaySelect()
    {
        SFXManager.Instance?.Play(selectCue);
    }

    public void PlayDrag()
    {
        SFXManager.Instance?.Play(dragCue);
    }

    public void PlayConfirm()
    {
        SFXManager.Instance?.Play(confirmCue);
    }

    public void PlayCancel()
    {
        SFXManager.Instance?.Play(cancelCue);
    }

    public void PlayDiscard()
    {
        SFXManager.Instance?.Play(discardCue);
    }
}
