using UnityEngine;

namespace Systems
{
    /// <summary>
    /// Global static lock to prevent multiple interactions (dialogues, teleports, minigames) from happening simultaneously.
    /// </summary>
    public static class InteractionLockManager
    {
        private static bool _isLocked = false;

        /// <summary>
        /// Try to acquire the interaction lock. Returns true if successful, false if already locked.
        /// </summary>
        public static bool TryLock()    
        {
            if (_isLocked)
            {
                Debug.Log("[InteractionLock] Interaction blocked - another interaction is already in progress.");
                return false;
            }

            _isLocked = true;
            Debug.Log("[InteractionLock] Lock acquired.");
            return true;
        }

        /// <summary>
        /// Release the interaction lock.
        /// </summary>
        public static void Unlock()
        {
            _isLocked = false;
            Debug.Log("[InteractionLock] Lock released.");
        }

        /// <summary>
        /// Check if the lock is currently active.
        /// </summary>
        public static bool IsLocked => _isLocked;
    }
}

