using System.Collections;
using UnityEngine;

public class MazePlayerController : MonoBehaviour
{
    [SerializeField] private GenerateMaze maze;      // drag your GenerateMaze object here
    [SerializeField] private float moveDuration = 0.15f;  // how fast it slides between cells

    private Vector2Int currentIndex;   // which cell (x,y) we’re in
    private bool isMoving = false;

    private void Start()
    {
        // Start in the bottom-left cell (0,0). Change if you want a different start.
        currentIndex = new Vector2Int(0, 0);

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
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            dirEnum = Room.Directions.TOP;
            delta = Vector2Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            dirEnum = Room.Directions.RIGHT;
            delta = Vector2Int.right;
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            dirEnum = Room.Directions.BOTTOM;
            delta = Vector2Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
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
        isMoving = false;
    }
}
