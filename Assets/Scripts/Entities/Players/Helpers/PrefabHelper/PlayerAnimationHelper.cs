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

    // Called from PlayerPrefab to register which image to animate
    public void SetShieldedBarImage(Image barImage)
    {
        currentShieldBarImage = barImage;

        if (barImage != null)
            originalSprite = barImage.sprite; // store default
    }

    // Start or stop the animation
    public void PlayShieldedBarAnimation(bool play)
    {
        if (shieldAnimRoutine != null)
        {
            StopCoroutine(shieldAnimRoutine);
            shieldAnimRoutine = null;
        }

        if (!play)
        {
            ResetToOriginalSprite();
            return;
        }

        if (shieldedHealthBarFrames != null && shieldedHealthBarFrames.Length > 0)
            shieldAnimRoutine = StartCoroutine(LoopShieldFrames());
    }

    public void ResetToOriginalSprite()
    {
        if (currentShieldBarImage != null && originalSprite != null)
            currentShieldBarImage.sprite = originalSprite;
    }

    private IEnumerator LoopShieldFrames()
    {
        int frame = 0;
        float delay = 1f / shieldAnimationFPS;

        while (true)
        {
            if (currentShieldBarImage)
                currentShieldBarImage.sprite = shieldedHealthBarFrames[frame];

            frame = (frame + 1) % shieldedHealthBarFrames.Length;
            yield return new WaitForSeconds(delay);
        }
    }

    // Other FX (still here for damage/heal pulses etc.)
    public void PlaySpawnAnimation() => Debug.Log("[AnimHelper] Spawn anim.");
    public void PlayDamageFlash() => Debug.Log("[AnimHelper] Damage flash.");
    public void PlayHealGlow() => Debug.Log("[AnimHelper] Heal glow.");
    public void PlayShieldPulse() => Debug.Log("[AnimHelper] Shield pulse.");
    public void PlayEnergyPulse() => Debug.Log("[AnimHelper] Energy pulse.");
}
