using System;
using UnityEngine;

/// <summary>
/// Centralized pause manager. Use this instead of setting Time.timeScale directly so
/// all systems (player input, clock timer, UI) can be paused/resumed consistently.
/// </summary>
public static class GlobalPause
{
    public static event Action<bool> OnPausedChanged;
    public static event Action<bool> OnMinigamePausedChanged;

    private static bool _isPaused = false;
    public static bool IsPaused => _isPaused;

    private static bool _isMinigamePaused = false;
    public static bool IsMinigamePaused => _isMinigamePaused;

    /// <summary>
    /// Set global paused state. This will toggle Time.timeScale and try to disable
    /// common input/UX systems: PlayerInput2D, ClockTimer, JournalUI.
    /// </summary>
    public static void SetPaused(bool pause)
    {
        if (_isPaused == pause) return;
        _isPaused = pause;

        // Toggle global timescale
        Time.timeScale = pause ? 0f : 1f;

        // Toggle player input for any PlayerInput2D instances in the scene
        var playerInputs = UnityEngine.Object.FindObjectsOfType<PlayerInput2D>(true);
        foreach (var pi in playerInputs)
        {
            try { pi.isInputEnabled = !pause; } catch { }
        }

        // Pause/resume clock timer if present
        var clock = UnityEngine.Object.FindObjectOfType<ClockTimer>(true);
        if (clock != null)
        {
            try { clock.PauseTimer(pause); } catch { }
        }

        // Disable journal UI input if present
        var journal = UnityEngine.Object.FindObjectOfType<JournalUI>(true);
        if (journal != null)
        {
            try { journal.SetInputEnabled(!pause); } catch { }
        }

        OnPausedChanged?.Invoke(pause);

        Debug.Log($"[GlobalPause] Paused set to {pause}");
    }

    public static void Toggle()
    {
        SetPaused(!_isPaused);
    }

    /// <summary>
    /// Sets minigame pause state. This pauses NPCs and ClockTimer, but does NOT pause
    /// player input or Time.timeScale. Use this during minigames so player can still interact
    /// but the overworld is paused.
    /// </summary>
    public static void SetMinigamePaused(bool pause)
    {
        if (_isMinigamePaused == pause) return;
        _isMinigamePaused = pause;

        // Pause/resume clock timer
        var clock = UnityEngine.Object.FindObjectOfType<ClockTimer>(true);
        if (clock != null)
        {
            try { clock.PauseTimer(pause); } catch { }
        }

        // Note: NPCs will check IsMinigamePaused in their Update() methods
        // We don't pause Time.timeScale or player input here
        
        OnMinigamePausedChanged?.Invoke(pause);
        Debug.Log($"[GlobalPause] Minigame paused set to {pause} (NPCs and Timer paused, player input still active)");
    }
}