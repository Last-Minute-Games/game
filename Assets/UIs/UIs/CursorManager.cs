using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [Header("Scaling")]
    public float cursorScale = 2f; 
    public bool lockHotspotToCenter = false; 

    private Texture2D _currentCursorTexture;
    private Vector2 _currentHotspot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: keep it across scenes
    }

    /// <summary>
    /// Use this instead of Cursor.SetCursor! 
    /// Scaling and hotspots are handled automatically.
    /// </summary>
    public void SetScaledCursor(Texture2D cursorTexture, Vector2 defaultHotspot)
    {
        _currentCursorTexture = cursorTexture;
        _currentHotspot = defaultHotspot;

        if (cursorTexture == null)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            return;
        }

        Texture2D scaledTex = cursorScale == 1f ? cursorTexture : ScaleTexture(cursorTexture, Mathf.RoundToInt(cursorTexture.width * cursorScale), Mathf.RoundToInt(cursorTexture.height * cursorScale));
        
        Vector2 finalHotspot = lockHotspotToCenter ? 
            new Vector2(scaledTex.width / 2f, scaledTex.height / 2f) : 
            new Vector2(defaultHotspot.x * cursorScale, defaultHotspot.y * cursorScale);

        Cursor.SetCursor(scaledTex, finalHotspot, CursorMode.ForceSoftware);
    }

    // Force an update if settings change via inspector during runtime
    private void OnValidate()
    {
        if (Application.isPlaying && _currentCursorTexture != null)
        {
            SetScaledCursor(_currentCursorTexture, _currentHotspot);
        }
    }

    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
        result.filterMode = FilterMode.Point; 

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                Color c = source.GetPixelBilinear((float)x / targetWidth, (float)y / targetHeight);
                result.SetPixel(x, y, c);
            }
        }
        result.Apply();
        return result;
    }
}
