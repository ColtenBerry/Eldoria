using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory; // Your runtime inventory system
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
        if (inventory == null) Debug.LogWarning("Inventory is null");
        Debug.Log("Attempting to add: " + item.name + ", " + amount);
        inventory.AddItem(item, amount);
    }

    public void RemoveItem(InventoryItem item, int amount = 1)
    {
        if (inventory == null) return;
        Debug.Log("Attempting to remove: " + item.name + ", " + amount);
        inventory.RemoveItem(item, amount);
    }

    public List<ItemStack> GetAllItems()
    {
        return Inventory.GetAllItems();
    }

    public void ClearAllItems()
    {
        inventory.ClearAllItems();
    }


}