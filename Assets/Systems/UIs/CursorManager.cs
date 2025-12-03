using UnityEngine;

public class SceneCursorSetter : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot;

    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }
}
