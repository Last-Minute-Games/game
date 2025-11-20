using TMPro;
using UnityEngine;
using DG.Tweening;

public class FloatingTooltip : MonoBehaviour
{
    public TextMeshProUGUI text;
    public CanvasGroup canvasGroup;

    public void Play(string message, Color color, Vector3 direction)
    {
        text.text = message;
        text.color = color;

        float duration = 1f;

        RectTransform rt = (RectTransform)transform;

        // Move (UI-friendly)
        rt.DOAnchorPos(rt.anchoredPosition + (Vector2)direction * 80f, duration)
          .SetEase(Ease.OutCubic);

        // Fade
        canvasGroup.DOFade(0f, duration);

        Destroy(gameObject, duration);
    }
}
