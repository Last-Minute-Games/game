using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
public class GenerateMaze : MonoBehaviour
{

    public int NumX => numX;
    public int NumY => numY;

    [SerializeField] GameObject roomPrefab;

    //The grid
    Room[,] rooms = null;

    [SerializeField] int numX = 10; int numY = 10;

    //Room width and height

    float roomWidth;
    float roomHeight;

    //The stack

    Stack<Room> stack = new Stack<Room>();

    bool generating = false;

    private bool roomsBuilt = false;


    private void GetRoomSize() {
        SpriteRenderer[] spriteRenderers = roomPrefab.GetComponentsInChildren<SpriteRenderer>();

        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        foreach (SpriteRenderer ren in spriteRenderers) {
            minBounds = Vector3.Min(minBounds, ren.bounds.min);

            maxBounds = Vector3.Max(maxBounds, ren.bounds.max);



        }
        roomWidth = maxBounds.x - minBounds.x;
        roomHeight = maxBounds.y - minBounds.y;
    }

        
         private void SetCamera(){
        
            Camera.main.transform.position = new Vector3( numX * (roomWidth - 1) / 2, numY * (roomHeight -1) / 2, -100.0f);

            float min_value = Mathf.Min(numX * (roomWidth - 1) , numY * (roomHeight - 1));
        Camera.main.orthographicSize = min_value * 0.75f;
        }


        
         
        private void Start() {

        BuildRoomsIfNeeded();

        /*
        GetRoomSize();

        rooms = new Room[numX, numY];

        for (int i = 0; i < numX; ++i) {
            for (int j = 0; j < numY; ++j) {
                GameObject room = Instantiate(roomPrefab, new Vector3(i * roomWidth, j * roomHeight, 0.0f), Quaternion.identity);

                //room.name = "Room_" + i.ToString() + "_" + j.ToString();
                rooms[i, j] = room.GetComponent<Room>();
                rooms[i, j].Index = new Vector2Int(i, j);
            }
        
        }

        //SetCamera();
        */
    }

    private void RemoveRoomWall(int x, int y, Room.Directions dir)
    {
        if (dir != Room.Directions.NONE) {
            rooms[x, y].SetDirFlag(dir, false);
        }
        //rooms[x,y].SetDirFlag(dir, false);

        Room.Directions opp = Room.Directions.NONE;
        switch (dir) { 
            case Room.Directions.TOP:
                if (y < numY - 1) {
                    opp = Room.Directions.BOTTOM;
                    ++y;
                }
                break;
            case Room.Directions.RIGHT:
                if (x < numX - 1)
                {
                    opp = Room.Directions.LEFT;
                    ++x;
                }
                break;
            case Room.Directions.BOTTOM:
                if (y > 0)
                {
                    opp = Room.Directions.TOP;
                    --y;
                }
                break;
            case Room.Directions.LEFT:
                if (x > 0)
                {
                    opp = Room.Directions.RIGHT;
                    --x;
                }
                break;
        }
        if (opp != Room.Directions.NONE)
        {
            rooms[x, y].SetDirFlag(opp, false);
        }

    }

    public List<Tuple<Room.Directions, Room>> GetNeighborsNotVisited(int cx, int cy)
    {
        List<Tuple<Room.Directions, Room>> neighbors = new List<Tuple<Room.Directions, Room>>();

        foreach (Room.Directions dir in Enum.GetValues(typeof(Room.Directions)))
        {
            int x = cx;
            int y = cy;

            switch (dir)
            {
                case Room.Directions.TOP:
                    if (y < numY - 1)
                    {
                        ++y;
                        if (!rooms[x, y].visited)
                        {
                            neighbors.Add(new Tuple<Room.Directions, Room>(Room.Directions.TOP, rooms[x, y]));
                        }
                    }
                    break;
                case Room.Directions.RIGHT:
                    if (x < numX - 1)
                    {
                        ++x;
                        if (!rooms[x, y].visited)
                        {
                            neighbors.Add(new Tuple<Room.Directions, Room>(Room.Directions.RIGHT, rooms[x, y]));
                        }
                    }
                    break;
                case Room.Directions.BOTTOM:
                    if (y > 0)
                    {
                        --y;
                        if (!rooms[x, y].visited)
                        {
                            neighbors.Add(new Tuple<Room.Directions, Room>(Room.Directions.BOTTOM, rooms[x, y]));
                        }
                    }
                    break;
                case Room.Directions.LEFT:
                    if (x > 0)
                    {
                        --x;
                        if (!rooms[x, y].visited)
                        {
                            neighbors.Add(new Tuple<Room.Directions, Room>(Room.Directions.LEFT, rooms[x, y]));
                        }
                    }
                    break;
            }
        }
        return neighbors; // Ensure this return statement is outside the foreach loop
    }

    private bool GenerateStep() {
        if (stack.Count == 0) {
            return true;
        }
        Room r = stack.Peek();
        var neighbors = GetNeighborsNotVisited(r.Index.x, r.Index.y);

        if (neighbors.Count != 0)
        {
            var index = 0;
            if (neighbors.Count > 1)
            {
                index = UnityEngine.Random.Range(0, neighbors.Count);
            }
            var item = neighbors[index];
            Room neighbor = item.Item2;
            neighbor.visited = true;
            RemoveRoomWall(r.Index.x, r.Index.y, item.Item1);

            stack.Push(neighbor);
        }

        else {
            stack.Pop();
        }

        return false;
    }

    public void CreateMaze() { 
        if(generating)
        {
            return;
        }

        BuildRoomsIfNeeded();

        Reset();

        RemoveRoomWall(0, 0, Room.Directions.BOTTOM);

        RemoveRoomWall(numX - 1, numY - 1, Room.Directions.RIGHT);

        stack.Push(rooms[0, 0]);

        StartCoroutine(Coroutine_Generate());

    }

    IEnumerator Coroutine_Generate() {
        generating = true;
        bool flag = false;

        while (!flag)
        {
            flag = GenerateStep();
            yield return new WaitForSeconds(0.05f);
        }
        generating = false;
    }

    private void Reset()
    {
        if (rooms == null)
        {
            Debug.LogError("GenerateMaze.Reset() called but rooms is null.");
            return;
        }
        for (int i = 0; i < numX; ++i) { 
            for(int j = 0; j < numY; ++j)
            {
                rooms[i,j].SetDirFlag(Room.Directions.TOP, true);
                rooms[i, j].SetDirFlag(Room.Directions.RIGHT, true);
                rooms[i, j].SetDirFlag(Room.Directions.BOTTOM, true);
                rooms[i, j].SetDirFlag(Room.Directions.LEFT, true);
                rooms[i, j].visited = false;

            }
        }
    }

    /*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (!generating) {
                CreateMaze();
            }
        }
    }
    */

    public Room GetRoom(Vector2Int index)
    {
        return rooms[index.x, index.y];
    }

    public Vector3 GetWorldPosition(Vector2Int index)
    {
        //old
        //return new Vector3(index.x * roomWidth, index.y * roomHeight, 0f);
        return transform.position + new Vector3(index.x * roomWidth,
                                            index.y * roomHeight,
                                            0f);
    }

    public Vector2Int GetMazeSize()
    {
        return new Vector2Int(numX, numY);
    }

    private void BuildRoomsIfNeeded()
    {
        if (roomsBuilt && rooms != null)
            return;

        GetRoomSize();

        rooms = new Room[numX, numY];

        for (int i = 0; i < numX; ++i)
        {
            for (int j = 0; j < numY; ++j)
            {
                //in ordere to move the maze from the scene
                GameObject room = Instantiate(roomPrefab, transform);
                room.transform.localPosition = new Vector3(i * roomWidth, j * roomHeight, 0.0f);

                //old maze start
                /*
                GameObject room = Instantiate(
                    roomPrefab,
                    new Vector3(i * roomWidth, j * roomHeight, 0.0f),
                    Quaternion.identity,
                    transform   // keep rooms grouped under this object
                );
                */
                room.name = "Room_" + i.ToString() + "_" + j.ToString();
                rooms[i, j] = room.GetComponent<Room>();
                rooms[i, j].Index = new Vector2Int(i, j);
            }
        }

        roomsBuilt = true;
    }



}

