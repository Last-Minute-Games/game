using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LootContainerInteract : Interactable
{
    [SerializeField] GameObject closedChest;
    [SerializeField] GameObject openedChest;
    [SerializeField] bool opened;
    public override void Interact(Character character)
    {
        // This method is meant to be overridden by subclasses.
        if(opened == false)
        {
            opened = true;
            closedChest.SetActive(false);
            openedChest.SetActive(true);

        }

    }
}
