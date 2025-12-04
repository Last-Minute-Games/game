using UnityEngine;
using TMPro;

/// <summary>
/// Handles scrolling credits with customizable content.
/// Credits scroll from bottom to top like traditional game credits.
/// </summary>
public class CreditsScroller : MonoBehaviour
{
    [Header("Credits Content")]
    [SerializeField] [TextArea(20, 50)]
    private string creditsText = @"CASTLE OF TIME

PRODUCED BY
Syrus Tolentino

ART AND ANIMATION
Murtaza Tayabali
Jennifer Ceja
Ngan Nguyen (External)
Linh Huynh

ENGINEER
Darrell Se
Aurelio Aguirre
Xing Huynh
Jester Santos
Henry Nguyen (External)

AUDIO
Melisa Unlu

SPECIAL THANKS
[Add your thanks here]

Thank you for playing!";

    [Header("UI References")]
    [SerializeField] private RectTransform creditsContainer;
    [SerializeField] private TextMeshProUGUI creditsTextUI;
    
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private float startYOffset = -500f; // Start below screen
    [SerializeField] private float endYOffset = 1500f; // End above screen
    
    [Header("Input Settings")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    
    private bool _isScrolling = false;
    private float _currentY;

    public bool IsScrolling => _isScrolling;

    private void Awake()
    {
        // Set up credits text
        if (creditsTextUI != null)
        {
            creditsTextUI.text = creditsText;
        }
        
        // Position credits container at start position
        if (creditsContainer != null)
        {
            creditsContainer.anchoredPosition = new Vector2(0, startYOffset);
        }
    }

    /// <summary>
    /// Start scrolling the credits
    /// </summary>
    public void StartScrolling()
    {
        if (_isScrolling)
        {
            Debug.LogWarning("[CreditsScroller] Already scrolling!");
            return;
        }
        
        Debug.Log("[CreditsScroller] Starting credits scroll");
        _isScrolling = true;
        _currentY = startYOffset;
        
        if (creditsContainer != null)
        {
            creditsContainer.anchoredPosition = new Vector2(0, _currentY);
        }
    }

    /// <summary>
    /// Stop scrolling the credits
    /// </summary>
    public void StopScrolling()
    {
        _isScrolling = false;
        Debug.Log("[CreditsScroller] Credits scroll stopped");
    }

    private void Update()
    {
        if (!_isScrolling)
            return;
        
        // Check for skip input
        if (allowSkip && Input.GetKeyDown(skipKey))
        {
            Debug.Log("[CreditsScroller] Credits skipped by player");
            SkipToEnd();
            return;
        }
        
        // Scroll the credits
        _currentY += scrollSpeed * Time.deltaTime;
        
        if (creditsContainer != null)
        {
            creditsContainer.anchoredPosition = new Vector2(0, _currentY);
        }
        
        // Check if we've reached the end
        if (_currentY >= endYOffset)
        {
            Debug.Log("[CreditsScroller] Credits reached end");
            StopScrolling();
        }
    }

    /// <summary>
    /// Skip directly to the end of the credits
    /// </summary>
    private void SkipToEnd()
    {
        _currentY = endYOffset;
        
        if (creditsContainer != null)
        {
            creditsContainer.anchoredPosition = new Vector2(0, _currentY);
        }
        
        StopScrolling();
    }

    /// <summary>
    /// Set custom credits text
    /// </summary>
    public void SetCreditsText(string text)
    {
        creditsText = text;
        
        if (creditsTextUI != null)
        {
            creditsTextUI.text = text;
        }
    }

    /// <summary>
    /// Set scroll speed
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }
}
