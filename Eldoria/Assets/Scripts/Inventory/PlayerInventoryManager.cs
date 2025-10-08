using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    [Header("References")]
    private Inventory inventory; // Your runtime inventory system
    [SerializeField] private List<StartingItem> startingItems = new();
    public Inventory Inventory => inventory;


    public void Awake()
    {
        inventory = new Inventory();

        foreach (StartingItem entry in startingItems)
        {
            if (entry.item != null)
            {
                inventory.AddItem(entry.item, entry.quantity);
            }
        }
    }


    protected void OnDisable()
    {
    }


    public void AddItem(InventoryItem item, int amount = 1)
    {
        inventory.AddItem(item, amount);
    }

    public void RemoveItem(InventoryItem item, int amount = 1)
    {
        Debug.Log("Attempting to remove: " + item.name);
        if (inventory == null) return;
        inventory.RemoveItem(item, amount);
    }


}