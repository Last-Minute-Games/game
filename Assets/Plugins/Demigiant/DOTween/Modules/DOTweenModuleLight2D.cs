using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DG.Tweening
{
    public static class DOTweenModuleLight2D
    {
        public static TweenerCore<float, float, FloatOptions> DOIntensity(this Light2D target, float endValue,
            float duration)
        {
            return DOTween.To(() => target.intensity, x => target.intensity = x, endValue, duration)
                .SetTarget(target);
        }

        public static TweenerCore<Color, Color, ColorOptions> DOColor(this Light2D target, Color endValue,
            float duration)
        {
            return DOTween.To(() => target.color, x => target.color = x, endValue, duration)
                .SetTarget(target);
        }
    }
}