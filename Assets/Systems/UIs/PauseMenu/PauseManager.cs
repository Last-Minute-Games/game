using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-900)]
public class PauseManager : MonoBehaviour
{
    public static PauseManager I { get; private set; }

    // Fired whenever pause changes; listeners get true=paused, false=unpaused
    public static event Action<bool> OnPauseChanged;

    // Optional: let PauseManager also control Time.timeScale (default true)
    [SerializeField] private bool useTimeScale = true;

    public bool IsPaused { get; private set; }
    public bool PauseAllowedInThisScene { get; private set; }

    // Ensure there is always one
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureInstance()
    {
        if (I == null && FindObjectOfType<PauseManager>() == null)
        {
            var go = new GameObject("PauseManager");
            go.AddComponent<PauseManager>();
            DontDestroyOnLoad(go);
        }
    }

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        RefreshSceneAllowance();
        SetPaused(false, fireEvent: false);
    }

    void OnDestroy()
    {
        if (I == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        RefreshSceneAllowance();
        // auto-unpause on scene load (safety)
        SetPaused(false);
    }

    void RefreshSceneAllowance()
    {
        PauseAllowedInThisScene = FindObjectOfType<AllowPause>() != null;
    }

    public void TogglePause()
    {
        if (!PauseAllowedInThisScene) return;
        SetPaused(!IsPaused);
    }

    public void Pause() { if (PauseAllowedInThisScene) SetPaused(true); }
    public void Resume() { SetPaused(false); }

    public void SetPaused(bool paused, bool fireEvent = true)
    {
        if (IsPaused == paused) return;
        IsPaused = paused;

        if (useTimeScale)
        {
            Time.timeScale = IsPaused ? 0f : 1f;
            // Optionally also: AudioListener.pause = IsPaused;
        }

        if (fireEvent) OnPauseChanged?.Invoke(IsPaused);
    }
}
