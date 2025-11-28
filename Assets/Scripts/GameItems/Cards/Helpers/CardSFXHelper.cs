using UnityEngine;

namespace GameItems.Cards.Helpers
{
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
        
        [Header("Card Effect Sounds")]
        public SFXCueData attackCue;
        public SFXCueData healCue;
        public SFXCueData blockCue;

        private void Awake()
        {
            Debug.Log($"[CardSFXHelper] Awake on {gameObject.name}. drawCue assigned: {drawCue != null}");
            if (drawCue != null)
            {
                Debug.Log($"[CardSFXHelper] drawCue name: {drawCue.cueName}, clips: {drawCue.audioClips?.Length ?? 0}");
            }
        }

        // ────────────────────────────────
        // Playback API
        // ────────────────────────────────

        public void PlayDraw()
        {
            if (SFXManager.Instance == null)
            {
                Debug.LogWarning("[CardSFXHelper] SFXManager.Instance is NULL! Cannot play draw sound.");
                return;
            }
        
            if (drawCue == null)
            {
                Debug.LogWarning("[CardSFXHelper] drawCue is NULL! Assign it in the Inspector.");
                return;
            }
        
            Debug.Log($"[CardSFXHelper] Playing draw sound: {drawCue.cueName}");
            SFXManager.Instance.Play(drawCue);
        }

        public void PlayHover()
        {
            if (SFXManager.Instance == null || hoverCue == null) return;
            SFXManager.Instance.Play(hoverCue);
        }

        public void PlaySelect()
        {
            if (SFXManager.Instance == null || selectCue == null) return;
            SFXManager.Instance.Play(selectCue);
        }

        public void PlayDrag()
        {
            if (SFXManager.Instance == null || dragCue == null) return;
            SFXManager.Instance.Play(dragCue);
        }

        public void PlayConfirm()
        {
            if (SFXManager.Instance == null || confirmCue == null) return;
            SFXManager.Instance.Play(confirmCue);
        }

        public void PlayCancel()
        {
            if (SFXManager.Instance == null || cancelCue == null) return;
            SFXManager.Instance.Play(cancelCue);
        }

        public void PlayDiscard()
        {
            if (SFXManager.Instance == null || discardCue == null) return;
            SFXManager.Instance.Play(discardCue);
        }

        public void PlayAttack()
        {
            if (SFXManager.Instance == null || attackCue == null) return;
            SFXManager.Instance.Play(attackCue);
        }

        public void PlayHeal()
        {
            if (SFXManager.Instance == null || healCue == null) return;
            SFXManager.Instance.Play(healCue);
        }

        public void PlayBlock()
        {
            if (SFXManager.Instance == null || blockCue == null) return;
            SFXManager.Instance.Play(blockCue);
        }
    }
}
