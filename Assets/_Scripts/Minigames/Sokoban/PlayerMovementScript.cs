using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerMovementScript : MonoBehaviour
{
    // Keeping this simple since the controller manages enable/disable state
    private bool ReadyToMove = true;

    void Update()
    {
        Vector2 moveinput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveinput.Normalize();

        if (moveinput.sqrMagnitude > 0.5f)
        {
            if (ReadyToMove)
            {
                ReadyToMove = false;
                Move(moveinput);
            }
        }
        else
        {
            ReadyToMove = true;
        }
    }

    public bool Move(Vector2 direction)
    {
        // Restrict movement to one axis at a time and ensure grid alignment
        if (Mathf.Abs(direction.x) < 0.5f)
        {
            direction.x = 0;
        }
        else
        {
            direction.y = 0;
        }
        direction.Normalize();

        if (Blocked(transform.position, direction))
        {
            return false;
        }
        else
        {
            // NEW FIX: Snap the player back to the grid before moving (prevents drift)
            transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), transform.position.z);
            transform.Translate(direction);
            return true;
        }
    }

    public bool Blocked(Vector3 position, Vector2 direction)
    {
        // Use the rounded position for the calculation to ensure perfect alignment check
        Vector3 roundedPosition = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), position.z);
        Vector2 newpos = new Vector2(roundedPosition.x, roundedPosition.y) + direction;

        // --- Dynamic Check for Walls (Obstacles) ---
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacles");
        foreach (var obj in obstacles)
        {
            // Compare against rounded whole number position
            if (Mathf.Round(obj.transform.position.x) == newpos.x && Mathf.Round(obj.transform.position.y) == newpos.y)
            {
                return true; // Wall is in the way
            }
        }

        // --- Dynamic Check for Boxes (ObjToPush) ---
        GameObject[] objToPush = GameObject.FindGameObjectsWithTag("ObjToPush");
        foreach (var box in objToPush)
        {
            // Compare against rounded whole number position
            if (Mathf.Round(box.transform.position.x) == newpos.x && Mathf.Round(box.transform.position.y) == newpos.y)
            {
                // Found a box, attempt to push it
                Push objpush = box.GetComponent<Push>();
                if (objpush && objpush.Move(direction))
                {
                    // The box moved successfully, so the player is NOT blocked
                    return false;
                }
                else
                {
                    // The box could not move (it hit something or was blocked), so the player IS blocked
                    return true;
                }
            }
        }

        return false;
    }
}
