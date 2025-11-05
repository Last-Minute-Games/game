using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Push : MonoBehaviour
{
    // These static references are now dynamic, keeping them commented out for clarity.
    // private GameObject[] Obstacles; 
    // private GameObject[] ObjToPush;

    void Start()
    {
        // ----------------------------------------------------
        // IMPORTANT: Ensure the box has a Rigidbody2D set to Kinematic 
        // and a Collider2D (not a trigger) for physics interactions with Goal triggers.
        // ----------------------------------------------------
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        // Prevents the box from moving by physics forces
        rb.isKinematic = true;
        // Prevents the box from rotating
        rb.freezeRotation = true;
    }

    // Move is the same as before, checking for walls and other boxes
    public bool Move(Vector2 direction)
    {
        // We will now find obstacles and other pushable objects dynamically
        if (ObjToBlocked(transform.position, direction))
        {
            return false;
        }
        else
        {
            transform.Translate(direction);
            return true;
        }
    }

    // ObjToBlocked is similar to the Player's Blocked method, but simpler
    public bool ObjToBlocked(Vector3 position, Vector2 direction)
    {
        Vector2 newpos = new Vector2(position.x, position.y) + direction;

        // Dynamic check for Walls (Obstacles)
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacles");
        foreach (var obj in obstacles)
        {
            if (obj.transform.position.x == newpos.x && obj.transform.position.y == newpos.y)
            {
                return true; // Wall is in the way
            }
        }

        // Dynamic check for other Boxes (ObjToPush)
        GameObject[] objToPush = GameObject.FindGameObjectsWithTag("ObjToPush");
        foreach (var otherBox in objToPush)
        {
            // IMPORTANT: Don't check against self!
            if (otherBox.gameObject != gameObject)
            {
                if (otherBox.transform.position.x == newpos.x && otherBox.transform.position.y == newpos.y)
                {
                    return true; // Another box is in the way
                }
            }
        }
        // The goal tile itself does NOT block the box, as the goal tile is a trigger.

        return false;
    }
}
