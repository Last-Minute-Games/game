using UnityEngine;
using TMPro;
using DG.Tweening;

public class ScreenFadeUI : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI resultText;

    [Header("Settings")]
    public float fadeDuration = 1.0f;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowMessage(string message, Color color)
    {
        resultText.text = message;
        resultText.color = color;

        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, fadeDuration);
    }
}
