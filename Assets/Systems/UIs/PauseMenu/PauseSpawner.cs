using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseSpawner : MonoBehaviour
{
    [SerializeField] private GameObject pauseUIPrefab;
    private GameObject instance;

    void Awake()
    {
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        if (PauseManager.I.PauseAllowedInThisScene)
            EnsureInstance();
        else
            HideInstance();
    }

    void EnsureInstance()
    {
        if (!instance)
        {
            var canvas = FindObjectOfType<Canvas>();
            instance = Instantiate(pauseUIPrefab, canvas ? canvas.transform : null);
            instance.SetActive(false); // hidden until paused
        }
    }

    void HideInstance()
    {
        if (instance) instance.SetActive(false);
    }
}
