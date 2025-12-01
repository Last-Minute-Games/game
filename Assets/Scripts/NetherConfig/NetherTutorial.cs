using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct NetherTutorialSlide
{
    public Sprite image;
    [TextArea(3, 6)]
    public string text;
    public int slideOrder; // lower number first
}

public class NetherTutorial : MonoBehaviour
{
    public static NetherTutorial Instance { get; private set; }
    
    [Header("Slides")]
    public NetherTutorialSlide[] slides;    

    private List<NetherTutorialSlide> _orderedSlides = new();
    private int _currentIndex = 0;

    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public Image slideImage;
    public TextMeshProUGUI slideText;
    public TextMeshProUGUI slideCounter;

    [Header("Buttons")]
    public Button nextButton;
    public Button prevButton;
    public Button startGameButton;

    [Header("SFX")]
    public AudioClip turnPageSFX;   // now only the clip — no AudioSource needed

    private bool _running = false;
    private bool _done = false;
    private bool _hasReachedLastSlide = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Start hidden
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        startGameButton.gameObject.SetActive(false);
    }

    // ------------------------------------------------------
    // Main Entry Point Called By BattleManager
    // ------------------------------------------------------
    public IEnumerator RunTutorial()
    {
        if (_running)
            yield break;

        _running = true;
        _done = false;

        _orderedSlides = new List<NetherTutorialSlide>(slides);
        _orderedSlides.Sort((a, b) => a.slideOrder.CompareTo(b.slideOrder));

        _currentIndex = 0;

        // Fade in
        yield return FadeCanvas(canvasGroup, 0f, 1f, 0.25f);

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        // Show first slide
        ShowSlide(_currentIndex);

        // Freeze game time
        Time.timeScale = 0f;

        // Wait until Start Game button is pressed
        yield return new WaitUntil(() => _done);

        // Unfreeze
        Time.timeScale = 1f;

        // Fade out
        yield return FadeCanvas(canvasGroup, 1f, 0f, 0.25f);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        _running = false;
    }

    private void Start()
    {
        nextButton.onClick.AddListener(NextSlide);
        prevButton.onClick.AddListener(PrevSlide);
        startGameButton.onClick.AddListener(FinishTutorial);
    }

    private void ShowSlide(int index)
    {
        NetherTutorialSlide slide = _orderedSlides[index];

        slideImage.sprite = slide.image;
        slideText.text = slide.text;
        slideCounter.text = $"{index + 1}/{_orderedSlides.Count}";

        prevButton.gameObject.SetActive(index > 0);
        nextButton.gameObject.SetActive(index < _orderedSlides.Count - 1);
        // If we're on the last slide, unlock the Start Game button permanently
        if (index == _orderedSlides.Count - 1)
            _hasReachedLastSlide = true;

        // Start button persists once unlocked
        startGameButton.gameObject.SetActive(_hasReachedLastSlide);
    }

    private void NextSlide()
    {
        if (_currentIndex >= _orderedSlides.Count - 1) return;
        _currentIndex++;
        PlaySound();
        ShowSlide(_currentIndex);
    }

    private void PrevSlide()
    {
        if (_currentIndex <= 0) return;
        _currentIndex--;
        PlaySound();
        ShowSlide(_currentIndex);
    }

    private void PlaySound()
    {
        if (turnPageSFX != null)
            SFXManager.Instance.PlayClip(turnPageSFX);
    }

    private void FinishTutorial()
    {
        _done = true;
    }

    private IEnumerator FadeCanvas(CanvasGroup group, float from, float to, float duration)
    {
        float t = 0f;
        group.alpha = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        group.alpha = to;
    }
}
