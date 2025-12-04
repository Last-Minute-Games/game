using System.Collections;
using UnityEngine;

public class MazePlayerController : MonoBehaviour
{

    [SerializeField] private MazePopupController popupController; // drag MazePopup here
    private Vector2Int endIndex;

    [SerializeField] private GenerateMaze maze;      // drag your GenerateMaze object here
    [SerializeField] private float moveDuration = 0.15f;  // how fast it slides between cells

    [Header("Movement Audio")]
    [SerializeField] private AudioSource moveAudioSource;
    [SerializeField] private AudioClip moveClip;

    private Vector2Int currentIndex;   // which cell (x,y) we�re in
    private bool isMoving = false;

    private void Awake()
    {
        // Auto-grab AudioSource on this object if you forget to assign it
        if (moveAudioSource == null)
        {
            moveAudioSource = GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // Start in the bottom-left cell (0,0). Change if you want a different start.
        ResetToStart();

        //currentIndex = new Vector2Int(0, 0);

        if (maze != null)
        {
            Vector2Int size = maze.GetMazeSize();   // uses NumX/NumY internally
            endIndex = new Vector2Int(size.x - 1, size.y - 1); // top-right cell
        }
        else
        {
            Debug.LogError("MazePlayerController: Maze reference not set!");
        }

        ResetToStart();
    }
    public void ResetToStart() {
        currentIndex = new Vector2Int(0, 0); // use (0,0) as maze start

        if (maze != null)
        {
            transform.position = maze.GetWorldPosition(currentIndex);
        }
        else
        {
            Debug.LogError("MazePlayerController: Maze reference not set!");
        }
    }

    private void Update()
    {
        if (isMoving || maze == null) return;

        Room.Directions dirEnum = Room.Directions.NONE;
        Vector2Int delta = Vector2Int.zero;

        // WASD / arrows
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            dirEnum = Room.Directions.TOP;
            delta = Vector2Int.up;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            dirEnum = Room.Directions.RIGHT;
            delta = Vector2Int.right;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            dirEnum = Room.Directions.BOTTOM;
            delta = Vector2Int.down;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            dirEnum = Room.Directions.LEFT;
            delta = Vector2Int.left;
        }

        if (dirEnum == Room.Directions.NONE) return;

        TryMove(dirEnum, delta);
    }

    private void TryMove(Room.Directions dirEnum, Vector2Int delta)
    {
        // bounds check
        Vector2Int mazeSize = maze.GetMazeSize();
        Vector2Int targetIndex = currentIndex + delta;

        if (targetIndex.x < 0 || targetIndex.y < 0 ||
            targetIndex.x >= mazeSize.x || targetIndex.y >= mazeSize.y)
        {
            return; // out of maze
        }

        Room currentRoom = maze.GetRoom(currentIndex);

        // If there is a wall in this direction, don't move
        if (currentRoom.HasWall(dirEnum))
        {
            return;
        }

        // Otherwise move smoothly to the next cell
        StartCoroutine(MoveToCell(targetIndex));
    }

    private IEnumerator MoveToCell(Vector2Int newIndex)
    {
        isMoving = true;

        PlayMoveSound();

        Vector3 startPos = transform.position;
        Vector3 endPos = maze.GetWorldPosition(newIndex);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        currentIndex = newIndex;
        transform.position = endPos;
        isMoving = false;

        if (currentIndex == endIndex && popupController != null)
        {
            popupController.EndMaze(true); // true = solved/win
        }
    }

    private void PlayMoveSound()
    {
        if (moveAudioSource != null && moveClip != null)
        {
            moveAudioSource.PlayOneShot(moveClip);
        }
    }

}
