using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Animates HUD elements with a flash/fade-in effect when triggered
/// Attach this to your HUD Canvas or individual HUD elements
/// </summary>
public class HudInitializer : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Duration of each flash cycle")]
    [SerializeField] private float flashDuration = 0.15f;
    
    [Tooltip("Number of times to flash before final fade")]
    [SerializeField] private int flashCount = 6;
    
    [Tooltip("Final fade-in duration")]
    [SerializeField] private float finalFadeDuration = 0.5f;
    
    [Tooltip("Delay before starting the animation")]
    [SerializeField] private float startDelay = 0.2f;
    
    [Header("Target Elements")]
    [Tooltip("Leave empty to animate all Image and Text components on this GameObject and children")]
    [SerializeField] private CanvasGroup[] targetCanvasGroups;
    
    [Tooltip("Automatically find all Image and Text components")]
    [SerializeField] private bool autoFindComponents = true;
    
    [Header("First Time Animation")]
    [Tooltip("If true, resets the flag on Awake for testing purposes")]
    [SerializeField] private bool resetFlagOnAwake = false;
    
    private CanvasGroup[] allCanvasGroups;
    private Graphic[] allGraphics;
    
    private const string HUD_SHOWN_FLAG = "hudshown";
    
    private void Awake()
    {
        // Reset flag for testing if needed
        if (resetFlagOnAwake)
        {
            if (GameFlags.HasFlag(HUD_SHOWN_FLAG))
            {
                GameFlags.RemoveFlag(HUD_SHOWN_FLAG);
                Debug.Log("[HudInitializer] Flag reset for testing");
            }
        }
        
        // Collect all components to animate
        if (autoFindComponents)
        {
            if (targetCanvasGroups == null || targetCanvasGroups.Length == 0)
            {
                // Find or create canvas groups for each graphic element
                allGraphics = GetComponentsInChildren<Graphic>(true);
                
                // Create a canvas group for each graphic if it doesn't have one
                foreach (var graphic in allGraphics)
                {
                    if (graphic.GetComponent<CanvasGroup>() == null)
                    {
                        graphic.gameObject.AddComponent<CanvasGroup>();
                    }
                }
                
                allCanvasGroups = GetComponentsInChildren<CanvasGroup>(true);
            }
            else
            {
                allCanvasGroups = targetCanvasGroups;
            }
        }
        else
        {
            allCanvasGroups = targetCanvasGroups;
        }
        
        // Start invisible
        foreach (var cg in allCanvasGroups)
        {
            if (cg != null)
            {
                cg.alpha = 0f;
            }
        }
    }
    
    private void Start()
    {
        // Check if HUD has already been shown before
        if (GameFlags.HasFlag(HUD_SHOWN_FLAG))
        {
            // HUD has been shown before, show it immediately without animation
            Debug.Log("[HudInitializer] HUD already shown before, displaying immediately");
            ShowImmediately();
        }
        else
        {
            // First time - stay hidden until triggered by cutscene/dialogue
            Debug.Log("[HudInitializer] First time - HUD staying hidden until triggered");
            // HUD remains hidden (alpha = 0) until TriggerAnimation() is called
        }
    }
    
    private IEnumerator InitializeHudAnimation()
    {
        // Wait for initial delay
        yield return new WaitForSeconds(startDelay);
        
        // Flash effect: rapid fade in/out
        for (int i = 0; i < flashCount; i++)
        {
            // Fade in quickly
            foreach (var cg in allCanvasGroups)
            {
                if (cg != null)
                {
                    cg.DOFade(1f, flashDuration * 0.5f).SetEase(Ease.Linear);
                }
            }
            yield return new WaitForSeconds(flashDuration * 0.5f);
            
            // Fade out quickly
            foreach (var cg in allCanvasGroups)
            {
                if (cg != null)
                {
                    cg.DOFade(0f, flashDuration * 0.5f).SetEase(Ease.Linear);
                }
            }
            yield return new WaitForSeconds(flashDuration * 0.5f);
        }
        
        // Final smooth fade in
        foreach (var cg in allCanvasGroups)
        {
            if (cg != null)
            {
                cg.DOFade(1f, finalFadeDuration).SetEase(Ease.OutQuad);
            }
        }
        
        yield return new WaitForSeconds(finalFadeDuration);
        
        // Set the flag so we don't animate again
        GameFlags.SetFlag(HUD_SHOWN_FLAG);
        Debug.Log("[HudInitializer] HUD initialization complete! Flag set.");
    }
    
    /// <summary>
    /// Call this to manually trigger the HUD animation
    /// This should be called from the wake-up cutscene dialogue
    /// </summary>
    public void TriggerAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(InitializeHudAnimation());
    }
    
    /// <summary>
    /// Immediately show all HUD elements without animation
    /// </summary>
    public void ShowImmediately()
    {
        StopAllCoroutines();
        foreach (var cg in allCanvasGroups)
        {
            if (cg != null)
            {
                cg.DOKill();
                cg.alpha = 1f;
            }
        }
    }
    
    /// <summary>
    /// Hide all HUD elements immediately
    /// </summary>
    public void HideImmediately()
    {
        StopAllCoroutines();
        foreach (var cg in allCanvasGroups)
        {
            if (cg != null)
            {
                cg.DOKill();
                cg.alpha = 0f;
            }
        }
    }
    
    /// <summary>
    /// Reset the HUD shown flag (useful for testing or new game scenarios)
    /// </summary>
    public void ResetHudShownFlag()
    {
        if (GameFlags.HasFlag(HUD_SHOWN_FLAG))
        {
            GameFlags.RemoveFlag(HUD_SHOWN_FLAG);
            Debug.Log("[HudInitializer] HUD shown flag has been reset");
        }
    }
}
