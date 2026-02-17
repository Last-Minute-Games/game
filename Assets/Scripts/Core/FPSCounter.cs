using UnityEngine;

/// <summary>
/// Displays FPS counter in the top-left corner of the screen.
/// Attach to any GameObject in the scene.
/// </summary>
public class FPSCounter : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Update interval in seconds")]
    [SerializeField] private float updateInterval = 0.5f;
    
    [Tooltip("Font size for the FPS display")]
    [SerializeField] private int fontSize = 20;
    
    [Tooltip("Position offset from top-left corner")]
    [SerializeField] private Vector2 offset = new Vector2(10, 10);
    
    [Tooltip("Key to toggle the FPS counter on/off")]
    [SerializeField] private KeyCode toggleKey = KeyCode.P;

    private float accum = 0f;
    private int frames = 0;
    private float timeLeft;
    private float fps;
    private bool isVisible = false;

    private GUIStyle style;

    void Start()
    {
        timeLeft = updateInterval;
        
        // Setup GUI style
        style = new GUIStyle();
        style.fontSize = fontSize;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;
    }

    void Update()
    {
        // Toggle visibility with P key
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
        }

        // Only update FPS calculations if visible
        if (isVisible)
        {
            timeLeft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            frames++;

            // Update FPS when interval has passed
            if (timeLeft <= 0f)
            {
                fps = accum / frames;
                timeLeft = updateInterval;
                accum = 0f;
                frames = 0;
            }
        }
    }

    void OnGUI()
    {
        // Only display if visible
        if (!isVisible) return;

        // Display FPS with color coding
        if (fps >= 60f)
            style.normal.textColor = Color.green;
        else if (fps >= 30f)
            style.normal.textColor = Color.yellow;
        else
            style.normal.textColor = Color.red;

        string text = $"FPS: {fps:F1}";
        GUI.Label(new Rect(offset.x, offset.y, 200, 50), text, style);
    }
}
