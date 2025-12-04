using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OverworldRiddleItem : MonoBehaviour
{
    [Tooltip("The popup controller that shows the riddle page.")]
    [SerializeField] private RiddlePopupController riddlePopup;

    [Tooltip("Key to press to read the riddle.")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;

    private void Reset()
    {
        // Make sure this collider behaves as a trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            // TODO: optionally show "Press E" prompt here
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // TODO: hide prompt here if you added one
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            if (riddlePopup != null)
            {
                riddlePopup.Show();
            }
        }
    }
}
