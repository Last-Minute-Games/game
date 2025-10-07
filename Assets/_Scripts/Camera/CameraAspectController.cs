using UnityEngine; // deprecated file

// do not use (for now)

[RequireComponent(typeof(Camera))]
public class CameraAspectController : MonoBehaviour
{
    [Tooltip("Target aspect ratio (width / height), e.g. 16:9 = 1.7777")]
    public float targetAspect = 16f / 9f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateAspect();
    }

    void UpdateAspect()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            // Add letterbox (horizontal bars)
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            // Add pillarbox (vertical bars)
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }

    void OnPreCull()
    {
        // Fix viewport clearing artifacts on edges
        GL.Clear(true, true, Color.black);
    }
}
