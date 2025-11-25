using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerEnergyUIHelper : MonoBehaviour
{
    [Header("References")]
    public TMP_Text energyText;
    public Image energyIcon;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color zeroEnergyColor = Color.gray;
    public Color overflowColor = new Color(1f, 0.9f, 0.3f); // soft yellow glow

    [Header("Pulse Settings")]
    public float pulseScale = 1.4f;
    public float pulseDuration = 0.2f;

    private Tween _pulseTween;
    private Tween _glowTween;
    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = energyText.transform.localScale;
    }

    public void UpdateEnergyUI(int current, int max)
    {
        // number stays white always
        energyText.color = Color.white;
        energyText.text = $"{current} / {max}";

        // -------------------------------------
        // CASE 1: Zero energy
        // -------------------------------------
        if (current <= 0)
        {
            energyIcon.color = zeroEnergyColor; // gray icon
            StopGlow();
            return;
        }

        // -------------------------------------
        // CASE 2: Normal energy (1 → max)
        // -------------------------------------
        if (current <= max)
        {
            energyIcon.color = normalColor;  // icon normal
            StopGlow();
            RunPulse();
            return;
        }

        // -------------------------------------
        // CASE 3: Overflow (> max)
        // -------------------------------------
        if (current > max)
        {
            energyIcon.color = overflowColor; // yellow glow icon
            RunGlowLoop();
            RunPulse();
        }
    }

    private void RunPulse()
    {
        _pulseTween?.Kill();

        energyText.transform.localScale = _originalScale;

        _pulseTween = energyText.transform
            .DOScale(_originalScale * pulseScale, pulseDuration)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutBack);
    }

    private void RunGlowLoop()
    {
        if (_glowTween != null && _glowTween.IsActive())
            return;

        _glowTween = energyIcon.DOFade(0.4f, 0.6f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopGlow()
    {
        if (_glowTween != null)
        {
            _glowTween.Kill();
            _glowTween = null;

            // reset back to normal alpha
            var c = energyIcon.color;
            c.a = 1f;
            energyIcon.color = c;
        }
    }
}
