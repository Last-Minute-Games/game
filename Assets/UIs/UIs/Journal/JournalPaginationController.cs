using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles pagination (Previous/Next) for a journal page with multiple entries.
/// Shows only unlocked entries and allows scrolling through them.
/// Supports looping, animations, and sound effects.
/// </summary>
public class JournalPaginationController : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("How many entries to show per page")]
    public int entriesPerPage = 1;
    
    [Tooltip("Enable looping (page 7 -> page 1, page 1 -> page 7)")]
    public bool enableLooping = true;
    
    [Header("UI References")]
    [Tooltip("Button to go to previous entry")]
    public Button previousButton;
    
    [Tooltip("Button to go to next entry")]
    public Button nextButton;
    
    [Tooltip("Optional: Text showing current page (e.g., '1 / 3')")]
    public TMP_Text pageCounterText;
    
    [Header("Entry Container")]
    [Tooltip("The parent GameObject containing all entry GameObjects")]
    public GameObject entriesContainer;
    
    [Header("Animation Settings")]
    [Tooltip("CanvasGroup for fade animations (typically on entriesContainer)")]
    public CanvasGroup pageCanvasGroup;
    
    [Tooltip("Duration of fade in/out animation")]
    public float fadeDuration = 0.3f;
    
    [Header("Custom Page Turn Animation (Optional)")]
    [Tooltip("Animator for custom page turn effects")]
    public Animator pageTurnAnimator;
    
    [Tooltip("Animation clip to play when turning pages (e.g., page flip)")]
    public AnimationClip pageTurnClip;
    
    [Tooltip("Trigger name for custom animation (if using Animator)")]
    public string pageTurnTrigger = "TurnPage";
    
    [Tooltip("Use custom animation instead of simple fade")]
    public bool useCustomAnimation = false;
    
    [Header("Audio")]
    [Tooltip("Play sound when turning pages")]
    public bool playSoundOnPageTurn = true;
    
    [Header("Auto-Detection")]
    [Tooltip("Automatically find all JournalEntry components in children")]
    public bool autoDetectEntries = true;
    
    [Tooltip("Or manually assign entries")]
    public List<JournalEntry> allEntries = new List<JournalEntry>();
    
    [Header("Manager Reference")]
    [Tooltip("Reference to the JournalManager to check unlock status")]
    public JournalManager journalManager;
    
    // Runtime
    private List<JournalEntry> unlockedEntries = new List<JournalEntry>();
    private int currentPage = 0;
    private int totalPages = 0;
    private bool isInitialized = false;
    private bool isAnimating = false;
    private EnvironmentSoundHandler _environmentSoundHandler;
    
    private void Start()
    {
        // Auto-detect entries if enabled
        if (autoDetectEntries && entriesContainer != null)
        {
            allEntries.Clear();
            allEntries.AddRange(entriesContainer.GetComponentsInChildren<JournalEntry>(true));
            Debug.Log($"[Pagination] Auto-detected {allEntries.Count} entries in {gameObject.name}");
        }
        
        // Auto-find CanvasGroup if not assigned
        if (pageCanvasGroup == null && entriesContainer != null)
        {
            pageCanvasGroup = entriesContainer.GetComponent<CanvasGroup>();
            if (pageCanvasGroup == null)
            {
                pageCanvasGroup = entriesContainer.AddComponent<CanvasGroup>();
            }
        }
        
        // Hook up buttons
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousPage);
        
        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);
        
        // Subscribe to unlock events
        if (journalManager != null)
        {
            journalManager.OnEntryUnlocked += OnEntryUnlocked;
        }
        
        // Find the EnvironmentSoundHandler
        _environmentSoundHandler = GameObject.Find("EnvironmentSoundHandler")?.GetComponent<EnvironmentSoundHandler>();
        if (_environmentSoundHandler == null)
            Debug.LogWarning("[JournalPaginationController] EnvironmentSoundHandler not found in scene");
        
        isInitialized = true;
        
        // Initial refresh
        RefreshUnlockedEntries();
        ShowCurrentPage();
    }
    
    private void OnDestroy()
    {
        if (journalManager != null)
        {
            journalManager.OnEntryUnlocked -= OnEntryUnlocked;
        }
    }
    
    private void OnEnable()
    {
        // Only refresh if we've been initialized (Start has been called)
        // OnEnable runs before Start, so skip it the first time
        if (!isInitialized)
            return;
            
        // Refresh whenever this page becomes active
        RefreshUnlockedEntries();
        ShowCurrentPage();
    }
    
    private void OnEntryUnlocked(string entryId)
    {
        // When any entry is unlocked, refresh the list
        RefreshUnlockedEntries();
        ShowCurrentPage();
    }
    
    /// <summary>
    /// Build a list of all unlocked entries
    /// </summary>
    private void RefreshUnlockedEntries()
    {
        unlockedEntries.Clear();
        
        if (journalManager == null)
        {
            Debug.LogWarning("[Pagination] No JournalManager assigned!");
            return;
        }
        
        foreach (var entry in allEntries)
        {
            if (entry == null) continue;
            
            if (journalManager.IsEntryUnlocked(entry.entryId))
            {
                unlockedEntries.Add(entry);
            }
        }
        
        // Calculate total pages
        totalPages = Mathf.CeilToInt((float)unlockedEntries.Count / entriesPerPage);
        if (totalPages < 1) totalPages = 1;
        
        // Clamp current page
        if (currentPage >= totalPages)
            currentPage = totalPages - 1;
        if (currentPage < 0)
            currentPage = 0;
    }
    
    /// <summary>
    /// Show the entries for the current page
    /// </summary>
    private void ShowCurrentPage()
    {
        // First, hide all entries
        foreach (var entry in allEntries)
        {
            if (entry != null)
                entry.gameObject.SetActive(false);
        }
        
        // If no unlocked entries, show nothing
        if (unlockedEntries.Count == 0)
        {
            UpdateButtonStates();
            UpdatePageCounter();
            return;
        }
        
        // Calculate which entries to show
        int startIndex = currentPage * entriesPerPage;
        int endIndex = Mathf.Min(startIndex + entriesPerPage, unlockedEntries.Count);
        
        // Show only the entries for this page
        for (int i = startIndex; i < endIndex; i++)
        {
            if (unlockedEntries[i] != null)
            {
                unlockedEntries[i].gameObject.SetActive(true);
                unlockedEntries[i].SetUnlocked(true);
            }
        }
        
        UpdateButtonStates();
        UpdatePageCounter();
    }
    
    /// <summary>
    /// Go to the previous page with animation and sound
    /// </summary>
    public void PreviousPage()
    {
        if (isAnimating) return;
        
        int targetPage;
        
        if (currentPage > 0)
        {
            targetPage = currentPage - 1;
        }
        else if (enableLooping && totalPages > 1)
        {
            // Loop to last page
            targetPage = totalPages - 1;
        }
        else
        {
            return; // Can't go back
        }
        
        StartCoroutine(AnimatePageChange(targetPage));
    }
    
    /// <summary>
    /// Go to the next page with animation and sound
    /// </summary>
    public void NextPage()
    {
        if (isAnimating) return;
        
        int targetPage;
        
        if (currentPage < totalPages - 1)
        {
            targetPage = currentPage + 1;
        }
        else if (enableLooping && totalPages > 1)
        {
            // Loop to first page
            targetPage = 0;
        }
        else
        {
            return; // Can't go forward
        }
        
        StartCoroutine(AnimatePageChange(targetPage));
    }
    
    /// <summary>
    /// Animate the page transition
    /// </summary>
    private IEnumerator AnimatePageChange(int targetPage)
    {
        isAnimating = true;
        
        // Play sound effect
        PlayPageTurnSound();
        
        if (useCustomAnimation && pageTurnAnimator != null && pageTurnClip != null)
        {
            // Use custom animation
            yield return StartCoroutine(PlayCustomPageTurnAnimation(targetPage));
        }
        else
        {
            // Use simple fade animation
            yield return StartCoroutine(PlayFadeAnimation(targetPage));
        }
        
        isAnimating = false;
    }
    
    /// <summary>
    /// Simple fade out -> change page -> fade in animation
    /// </summary>
    private IEnumerator PlayFadeAnimation(int targetPage)
    {
        if (pageCanvasGroup == null)
        {
            // No canvas group, just change page instantly
            currentPage = targetPage;
            ShowCurrentPage();
            yield break;
        }
        
        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeDuration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (fadeDuration / 2f);
            pageCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        pageCanvasGroup.alpha = 0f;
        
        // Change page while invisible
        currentPage = targetPage;
        ShowCurrentPage();
        
        // Fade in
        elapsed = 0f;
        while (elapsed < fadeDuration / 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (fadeDuration / 2f);
            pageCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        pageCanvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// Play custom page turn animation (e.g., page flip)
    /// TODO: Implement your custom page flip animation here
    /// </summary>
    private IEnumerator PlayCustomPageTurnAnimation(int targetPage)
    {
        // Trigger the animation
        if (!string.IsNullOrEmpty(pageTurnTrigger))
        {
            pageTurnAnimator.SetTrigger(pageTurnTrigger);
        }
        
        // Wait for half the animation duration
        float animDuration = pageTurnClip != null ? pageTurnClip.length : 0.5f;
        yield return new WaitForSeconds(animDuration / 2f);
        
        // Change page at the midpoint of the animation
        currentPage = targetPage;
        ShowCurrentPage();
        
        // Wait for the rest of the animation
        yield return new WaitForSeconds(animDuration / 2f);
    }
    
    /// <summary>
    /// Play the page turn sound effect
    /// </summary>
    private void PlayPageTurnSound()
    {
        if (playSoundOnPageTurn)
        {
            try
            {
                if (_environmentSoundHandler != null)
                {
                    // Play the journal sound for page turns (using true for the "open" sound)
                    _environmentSoundHandler.PlayJournalSound(true);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[JournalPaginationController] Failed to play page turn sound: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Jump to a specific page (0-indexed) - no animation
    /// </summary>
    public void GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < totalPages)
        {
            currentPage = pageIndex;
            ShowCurrentPage();
        }
    }
    
    /// <summary>
    /// Update the enabled/disabled state of navigation buttons
    /// </summary>
    private void UpdateButtonStates()
    {
        if (enableLooping && totalPages > 1)
        {
            // Always enable both buttons when looping
            if (previousButton != null)
                previousButton.interactable = true;
            
            if (nextButton != null)
                nextButton.interactable = true;
        }
        else
        {
            // Normal behavior - disable at boundaries
            if (previousButton != null)
            {
                previousButton.interactable = (currentPage > 0);
            }
            
            if (nextButton != null)
            {
                nextButton.interactable = (currentPage < totalPages - 1);
            }
        }
    }
    
    /// <summary>
    /// Update the page counter text (e.g., "1 / 3")
    /// </summary>
    private void UpdatePageCounter()
    {
        if (pageCounterText != null)
        {
            if (unlockedEntries.Count == 0)
            {
                pageCounterText.text = "No entries unlocked";
            }
            else
            {
                pageCounterText.text = $"{currentPage + 1} / {totalPages}";
            }
        }
    }
    
    /// <summary>
    /// Get the current page index
    /// </summary>
    public int CurrentPage => currentPage;
    
    /// <summary>
    /// Get the total number of pages
    /// </summary>
    public int TotalPages => totalPages;
    
    /// <summary>
    /// Get the number of unlocked entries
    /// </summary>
    public int UnlockedCount => unlockedEntries.Count;
}
