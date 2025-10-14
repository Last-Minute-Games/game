using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenPulseEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image vignetteImage;

    [Header("Settings")]
    [Range(0f, 1f)] public float lowHealthThreshold = 0.35f;
    public Color pulseColor = new Color(1f, 0f, 0f, 0.25f); // less harsh
    public float pulseSpeed = 2.5f; // slower, smoother

    private Player player;
    private Tween pulseTween;
    private readonly Color clearColor = new Color(0, 0, 0, 0);

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        if (vignetteImage != null)
        {
            vignetteImage.color = clearColor;
            vignetteImage.raycastTarget = false;
        }

        Debug.Log($"🩸 ScreenPulseEffect initialized. Player={player != null}, Image={vignetteImage != null}");
    }

    private void Update()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
            if (player == null) return;
            Debug.Log("🧍 Player found by ScreenPulseEffect!");
        }

        if (vignetteImage == null) return;

        float ratio = (float)player.currentHealth / player.maxHealth;

        if (ratio <= lowHealthThreshold && player.currentHealth > 0)
        {
            if (pulseTween == null || !pulseTween.IsActive())
            {
                pulseTween = vignetteImage
                    .DOColor(pulseColor, pulseSpeed)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        else
        {
            if (pulseTween != null && pulseTween.IsActive())
            {
                pulseTween.Kill();
                vignetteImage.DOColor(clearColor, 0.6f);
            }
        }
    }

    public void StopAllEffects()
    {
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();
        vignetteImage.color = clearColor;
    }

    public void FadeToBlack(float fadeDuration = 2f)
    {
        StopAllEffects();
        vignetteImage.raycastTarget = true; // prevent further clicks during fade
        vignetteImage.DOColor(Color.black, fadeDuration).SetEase(Ease.InOutSine);
    }
}
