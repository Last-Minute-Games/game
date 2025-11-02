using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Handles pagination (Previous/Next) for a journal page with multiple entries.
/// Shows only unlocked entries and allows scrolling through them.
/// </summary>
public class JournalPaginationController : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("How many entries to show per page")]
    public int entriesPerPage = 1;
    
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
    
    private void Start()
    {
        // Auto-detect entries if enabled
        if (autoDetectEntries && entriesContainer != null)
        {
            allEntries.Clear();
            allEntries.AddRange(entriesContainer.GetComponentsInChildren<JournalEntry>(true));
            Debug.Log($"[Pagination] Auto-detected {allEntries.Count} entries in {gameObject.name}");
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
        
        Debug.Log($"[Pagination] RefreshUnlockedEntries - checking {allEntries.Count} total entries");
        
        foreach (var entry in allEntries)
        {
            if (entry == null)
            {
                Debug.LogWarning("[Pagination] Found null entry in allEntries!");
                continue;
            }
            
            bool isUnlocked = journalManager.IsEntryUnlocked(entry.entryId);
            Debug.Log($"[Pagination] Entry '{entry.entryId}' unlocked: {isUnlocked}");
            
            if (isUnlocked)
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
        
        Debug.Log($"[Pagination] Found {unlockedEntries.Count} unlocked entries, {totalPages} pages, entriesPerPage={entriesPerPage}");
    }
    
    /// <summary>
    /// Show the entries for the current page
    /// </summary>
    private void ShowCurrentPage()
    {
        Debug.Log($"[Pagination] ShowCurrentPage called. Page {currentPage}, unlocked count: {unlockedEntries.Count}");
        
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
        
        Debug.Log($"[Pagination] Showing entries from index {startIndex} to {endIndex - 1}");
        
        // Show only the entries for this page
        for (int i = startIndex; i < endIndex; i++)
        {
            if (unlockedEntries[i] != null)
            {
                Debug.Log($"[Pagination] Activating entry {i}: {unlockedEntries[i].entryId}");
                unlockedEntries[i].gameObject.SetActive(true);
                unlockedEntries[i].SetUnlocked(true); // Ensure they're in unlocked state
            }
        }
        
        UpdateButtonStates();
        UpdatePageCounter();
    }
    
    /// <summary>
    /// Go to the previous page
    /// </summary>
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ShowCurrentPage();
        }
    }
    
    /// <summary>
    /// Go to the next page
    /// </summary>
    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            ShowCurrentPage();
        }
    }
    
    /// <summary>
    /// Jump to a specific page (0-indexed)
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
        if (previousButton != null)
        {
            previousButton.interactable = (currentPage > 0);
        }
        
        if (nextButton != null)
        {
            nextButton.interactable = (currentPage < totalPages - 1);
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
