using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{   
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this as T;
        
    }

    protected virtual void OnApplicationQuit()
    {
        Instance = null;
        Destroy(gameObject);
    }

}

public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour {

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
