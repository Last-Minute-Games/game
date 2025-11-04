using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class RoundTransitionUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private float fadeDuration = 0.75f;
    [SerializeField] private float holdDuration = 1.0f;

    private void Awake()
    {
        background.color = new Color(0, 0, 0, 0);
        roundText.alpha = 0;
    }

    public IEnumerator FadeIn()
    {
        yield return background.DOFade(1f, fadeDuration).WaitForCompletion();
    }

    public IEnumerator ShowRoundText(int roundNumber)
    {
        roundText.text = $"ROUND {roundNumber}";
        yield return roundText.DOFade(1f, 0.4f).WaitForCompletion();
        yield return new WaitForSeconds(holdDuration);
        yield return roundText.DOFade(0f, 0.3f).WaitForCompletion();
    }

    public IEnumerator FadeOut()
    {
        yield return background.DOFade(0f, fadeDuration).WaitForCompletion();
    }
}
