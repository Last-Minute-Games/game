using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Interactable : MonoBehaviour
{
    public virtual void Interact(Character character)
    {
        // This method is meant to be overridden by subclasses.
        Debug.Log("Interacted with " + gameObject.name);

    }


}
