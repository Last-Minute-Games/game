using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager I { get; private set; }

    [Header("Video")]
    [SerializeField] private int defaultScreenMode = 1; // 0=Windowed, 1=Borderless, 2=Fullscreen

    // Current values (public read-only; change through Apply* methods)
    public float MasterVolume { get; private set; } // 0..1
    public ScreenModeType ScreenMode { get; private set; } // Windowed, Borderless, or Fullscreen
    public int ResolutionIndex { get; private set; } // index into ResList

    public readonly List<(int w, int h)> ResList = new();

    public enum ScreenModeType
    {
        Windowed = 0,
        Borderless = 1,
        Fullscreen = 2
    }

    // PlayerPrefs keys
    const string PP_VOL_M = "vol_master";
    const string PP_SCREEN_MODE = "vid_screen_mode";
    const string PP_RES_W = "vid_res_w";
    const string PP_RES_H = "vid_res_h";

    /// <summary>
    /// Get or auto-create the SettingsManager instance
    /// </summary>
    public static SettingsManager GetOrCreate()
    {
        if (I == null)
        {
            var go = new GameObject("SettingsManager");
            I = go.AddComponent<SettingsManager>();
            DontDestroyOnLoad(go);
            Debug.Log("[SettingsManager] Auto-created instance");
        }
        return I;
    }

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        BuildResolutionList();
        LoadAndApplyAll();
    }

    // --------- Load/Save ----------
    void LoadAndApplyAll()
    {
        MasterVolume = PlayerPrefs.GetFloat(PP_VOL_M, 0.75f);
        ScreenMode = (ScreenModeType)Mathf.Clamp(PlayerPrefs.GetInt(PP_SCREEN_MODE, defaultScreenMode), 0, 2);

        // Find saved resolution (fallback to current, then highest)
        int w = PlayerPrefs.GetInt(PP_RES_W, 0);
        int h = PlayerPrefs.GetInt(PP_RES_H, 0);

        if (w > 0 && h > 0)
            ResolutionIndex = Mathf.Max(0, ResList.FindIndex(r => r.w == w && r.h == h));
        else
        {
            var cur = Screen.currentResolution;
            int idx = ResList.FindIndex(r => r.w == cur.width && r.h == cur.height);
            ResolutionIndex = idx >= 0 ? idx : 0;
        }

        // Apply to systems
        ApplyMaster(MasterVolume);
        ApplyScreenMode((int)ScreenMode);
        ApplyResolution(ResolutionIndex, savePrefs: false);
    }

    public void Save()
    {
        PlayerPrefs.SetFloat(PP_VOL_M, MasterVolume);
        PlayerPrefs.SetInt(PP_SCREEN_MODE, (int)ScreenMode);
        var r = ResList[Mathf.Clamp(ResolutionIndex, 0, ResList.Count - 1)];
        PlayerPrefs.SetInt(PP_RES_W, r.w);
        PlayerPrefs.SetInt(PP_RES_H, r.h);
        PlayerPrefs.Save();
    }

    // --------- Apply methods (call these from UI) ----------
    public void ApplyMaster(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        AudioListener.volume = MasterVolume;
        Save();
    }

    public void ApplyScreenMode(int mode)
    {
        ScreenMode = (ScreenModeType)Mathf.Clamp(mode, 0, 2);
        
        FullScreenMode unityMode;
        switch (ScreenMode)
        {
            case ScreenModeType.Windowed:
                unityMode = FullScreenMode.Windowed;
                break;
            case ScreenModeType.Borderless:
                unityMode = FullScreenMode.FullScreenWindow;
                break;
            case ScreenModeType.Fullscreen:
                unityMode = FullScreenMode.ExclusiveFullScreen;
                break;
            default:
                unityMode = FullScreenMode.FullScreenWindow;
                break;
        }

        Screen.fullScreenMode = unityMode;
        Save();

        // Re-enforce current resolution in the new mode
        var r = ResList[Mathf.Clamp(ResolutionIndex, 0, ResList.Count - 1)];
        Screen.SetResolution(r.w, r.h, unityMode);
    }

    public void ApplyResolution(int index, bool savePrefs = true)
    {
        if (ResList.Count == 0) return;
        ResolutionIndex = Mathf.Clamp(index, 0, ResList.Count - 1);
        var (w, h) = ResList[ResolutionIndex];
        
        FullScreenMode currentMode = Screen.fullScreenMode;
        Screen.SetResolution(w, h, currentMode);
        if (savePrefs) Save();
    }

    // --------- Helpers ----------
    void BuildResolutionList()
    {
        // Get all unique resolutions (width x height)
        // Unity will automatically use the best refresh rate for the monitor
        var uniqueResolutions = Screen.resolutions
            .GroupBy(r => (r.width, r.height))
            .Select(g => g.First())
            .OrderByDescending(r => r.width * r.height)
            .ToList();

        ResList.Clear();
        foreach (var r in uniqueResolutions)
        {
            ResList.Add((r.width, r.height));
        }

        if (ResList.Count == 0) // fallback just in case
            ResList.Add((Screen.currentResolution.width, Screen.currentResolution.height));
    }
}
