using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerAnimationHelper : MonoBehaviour
{
    [Header("Shielded Health Bar Animation")]
    [Tooltip("Frames for the shielded (blue) health bar animation.")]
    public Sprite[] shieldedHealthBarFrames;

    [Header("Default sprite")]
    private Sprite originalSprite;

    [Tooltip("FPS for shield bar animation.")]
    public float shieldAnimationFPS = 12f;

    private Image currentShieldBarImage;
    private Coroutine shieldAnimRoutine;
    private bool isShieldActive; // Track if shield is currently active

    // Called from PlayerPrefab to register which image to animate
    public void SetShieldedBarImage(Image barImage)
    {
        currentShieldBarImage = barImage;

        if (barImage != null)
            originalSprite = barImage.sprite; // store default
    }

    // Start or stop the animation based on block value
    // play = true when block > 0, false when block == 0
    public void PlayShieldedBarAnimation(bool play)
    {
        if (shieldAnimRoutine != null)
        {
            StopCoroutine(shieldAnimRoutine);
            shieldAnimRoutine = null;
        }

        if (!play)
        {
            // Block reached 0, reset to original
            isShieldActive = false;
            ResetToOriginalSprite();
            return;
        }

        // Block > 0
        if (shieldedHealthBarFrames != null && shieldedHealthBarFrames.Length > 0)
        {
            if (!isShieldActive)
            {
                // First time getting shield, play animation once
                isShieldActive = true;
                shieldAnimRoutine = StartCoroutine(PlayShieldAnimationOnce());
            }
            else
            {
                // Already has shield, just hold final frame
                HoldFinalFrame();
            }
        }
    }

    public void ResetToOriginalSprite()
    {
        if (currentShieldBarImage != null && originalSprite != null)
            currentShieldBarImage.sprite = originalSprite;
    }

    private void HoldFinalFrame()
    {
        if (currentShieldBarImage != null && shieldedHealthBarFrames.Length > 0)
        {
            currentShieldBarImage.sprite = shieldedHealthBarFrames[shieldedHealthBarFrames.Length - 1];
        }
    }

    private IEnumerator PlayShieldAnimationOnce()
    {
        float delay = 1f / shieldAnimationFPS;

        // Play through all frames once
        for (int frame = 0; frame < shieldedHealthBarFrames.Length; frame++)
        {
            if (currentShieldBarImage)
                currentShieldBarImage.sprite = shieldedHealthBarFrames[frame];

            yield return new WaitForSeconds(delay);
        }

        // After animation completes, hold the final frame
        HoldFinalFrame();
    }

    // Other FX (still here for damage/heal pulses etc.)
    public void PlaySpawnAnimation() => Debug.Log("[AnimHelper] Spawn anim.");
    public void PlayDamageFlash() => Debug.Log("[AnimHelper] Damage flash.");
    public void PlayHealGlow() => Debug.Log("[AnimHelper] Heal glow.");
    public void PlayShieldPulse() => Debug.Log("[AnimHelper] Shield pulse.");
    public void PlayEnergyPulse() => Debug.Log("[AnimHelper] Energy pulse.");
}
