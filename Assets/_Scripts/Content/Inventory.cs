
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Inventory
{
    public List<Item> items = new List<Item>();

    public void AddItem(Item item)
    {
        items.Add(item);
        Debug.Log($"Added {item.itemName} to inventory.");
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);
        Debug.Log($"Removed {item.itemName} from inventory.");
    }
}

[System.Serializable]
public class Item
{
    public string itemName;
    public Sprite icon;
}
