using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    // public event Action<List<ItemStack>> OnInventoryChanged;
    [SerializeField] private List<ItemStack> itemStacks = new();

    public void AddItem(InventoryItem item, int amount = 1)
    {
        if (item.isStackable)
        {
            var stack = itemStacks.FirstOrDefault(s => s.item == item && s.quantity < s.MaxStackSize);
            if (stack != null)
            {
                int spaceLeft = stack.MaxStackSize - stack.quantity;
                int toAdd = Mathf.Min(spaceLeft, amount);
                stack.quantity += toAdd;
                amount -= toAdd;
            }

            while (amount > 0)
            {
                int toAdd = Mathf.Min(item.maxStackSize, amount);
                itemStacks.Add(new ItemStack(item, toAdd));
                Debug.Log("Stack added");
                amount -= toAdd;
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
                itemStacks.Add(new ItemStack(item, 1));
        }
        // OnInventoryChanged.Invoke(itemStacks);
    }

    public void RemoveItem(InventoryItem item, int amount = 1)
    {
        for (int i = itemStacks.Count - 1; i >= 0 && amount > 0; i--)
        {
            var stack = itemStacks[i];
            if (stack.item == item)
            {
                int toRemove = Mathf.Min(stack.quantity, amount);
                stack.quantity -= toRemove;
                amount -= toRemove;

                if (stack.quantity <= 0)
                    itemStacks.RemoveAt(i);
            }
        }
        //  OnInventoryChanged?.Invoke(itemStacks);
    }

    public List<ItemStack> GetAllItems() => new(itemStacks);
}


[System.Serializable]
public class ItemStack
{
    public InventoryItem item; // Reference to the ScriptableObject
    public int quantity;

    public ItemStack(InventoryItem item, int quantity = 1)
    {
        this.item = item;
        this.quantity = quantity;
    }

    public bool IsStackable => item.isStackable; // Add this to InventoryItem
    public int MaxStackSize => item.maxStackSize; // Also add to InventoryItem
}

