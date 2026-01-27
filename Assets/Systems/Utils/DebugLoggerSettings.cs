using UnityEngine;

[CreateAssetMenu(fileName = "DebugLoggerSettings", menuName = "Debug/Logger Settings")]
public class DebugLoggerSettings : ScriptableObject
{
    [Header("Toggle Debug Logs (Editor Only)")]
    [Tooltip("All logs are disabled by default and only work in Editor")]
    public DebugLogger.Settings settings = new DebugLogger.Settings();
}
