using UnityEngine;

public class MazePopupActivator : MonoBehaviour
{
    [Tooltip("Reference to the MazePopupController in your scene.")]
    public MazePopupController mazePopup;

    bool playerInRange = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("MazePopupActivator: E pressed, calling Show()");
            mazePopup.Show();
        }
    }
}
