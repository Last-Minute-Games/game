using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Handles the special cutscene that plays after day.five is completed.
/// This cutscene plays BEFORE returning to the overworld.
/// </summary>
public class DayFiveCutsceneManager : MonoBehaviour
{
    [Header("Cutscene Settings")]
    [SerializeField] private string cutsceneSceneName = "DayFiveCutscene";
    [SerializeField] private float delayBeforeCutscene = 2f;
    
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    
    private static bool _shouldPlayCutscene = false;
    
    /// <summary>
    /// Call this to mark that the day five cutscene should play
    /// </summary>
    public static void TriggerDayFiveCutscene()
    {
        _shouldPlayCutscene = true;
        Debug.Log("[DayFiveCutsceneManager] Day five cutscene marked to play");
    }
    
    /// <summary>
    /// Check if the day five cutscene should play
    /// </summary>
    public static bool ShouldPlayCutscene()
    {
        return _shouldPlayCutscene;
    }
    
    /// <summary>
    /// Reset the cutscene flag (called after cutscene plays)
    /// </summary>
    public static void ResetCutsceneFlag()
    {
        _shouldPlayCutscene = false;
        Debug.Log("[DayFiveCutsceneManager] Day five cutscene flag reset");
    }
    
    /// <summary>
    /// Play the day five cutscene sequence
    /// </summary>
    public IEnumerator PlayCutscene()
    {
        Debug.Log("[DayFiveCutsceneManager] Starting day five cutscene");
        
        // Wait a moment
        yield return new WaitForSeconds(delayBeforeCutscene);
        
        // Fade to black if we have a fade canvas
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
            float elapsed = 0f;
            float startAlpha = fadeCanvasGroup.alpha;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeDuration);
                yield return null;
            }
            
            fadeCanvasGroup.alpha = 1f;
        }
        
        // Load the cutscene scene
        Debug.Log($"[DayFiveCutsceneManager] Loading cutscene scene: {cutsceneSceneName}");
        ResetCutsceneFlag(); // Clear flag before loading
        
        SceneManager.LoadScene(cutsceneSceneName);
    }
}
