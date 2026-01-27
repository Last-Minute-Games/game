using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Global static lock to prevent multiple interactions (dialogues, teleports, minigames) from happening simultaneously.
    /// </summary>
    public static class InteractionLockManager
    {
        private static bool _isLocked = false;
        private static bool _enableDebugLogs = false; // Toggle via code or debugger

        /// <summary>
        /// Try to acquire the interaction lock. Returns true if successful, false if already locked.
        /// </summary>
        public static bool TryLock()    
        {
            if (_isLocked)
            {
                LogDebug("Interaction blocked - another interaction is already in progress.");
                return false;
            }

            _isLocked = true;
            LogDebug("Lock acquired.");
            return true;
        }

        /// <summary>
        /// Release the interaction lock.
        /// </summary>
        public static void Unlock()
        {
            _isLocked = false;
            LogDebug("Lock released.");
        }

        /// <summary>
        /// Check if the lock is currently active.
        /// </summary>
        public static bool IsLocked => _isLocked;
        
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private static void LogDebug(string message)
        {
            if (_enableDebugLogs)
                UnityEngine.Debug.Log($"[InteractionLock] {message}");
        }
    }
}

