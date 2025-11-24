using UnityEngine;

public class MazeEntrance : MonoBehaviour
{
    [SerializeField] private GameObject mazeRoot;        // parent with GenerateMaze + MazePlayer
    [SerializeField] private GameObject overworldPlayer; // MAIN PLAYER object
    [SerializeField] private Transform mazeStartPoint;   // where MazePlayer should appear (optional)

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        // Press E to enter when standing on the entrance
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            EnterMaze();
        }
    }

    private void EnterMaze()
    {
        // Turn off overworld player
        overworldPlayer.SetActive(false);

        // Turn on maze objects
        mazeRoot.SetActive(true);

        // Place maze player at starting cell (if you want to override)
        MazePlayerController mazePlayer = mazeRoot.GetComponentInChildren<MazePlayerController>();
        if (mazePlayer != null && mazeStartPoint != null)
        {
            mazePlayer.transform.position = mazeStartPoint.position;
        }

        // If your GenerateMaze uses Space to generate, you can also call a public method here
        // mazeRoot.GetComponentInChildren<GenerateMaze>().Generate();
    }
}
