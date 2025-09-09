using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryManager : MenuController
{
    [Header("References")]
    private Inventory playerInventory; // Your runtime inventory system
    [SerializeField] private InventoryUIController uiController; // Decoupled UI controller

    public void Initialize(Inventory inventory)
    {
        playerInventory = inventory;
        if (inventory != null)
            inventory.OnInventoryChanged += HandleInventoryChanged;

        // Initial sync
        HandleInventoryChanged(inventory.GetAllItems());
    }


    protected override void OnDisable()
    {
        base.OnDisable();
        if (playerInventory != null)
            playerInventory.OnInventoryChanged -= HandleInventoryChanged;
    }

    private void HandleInventoryChanged(List<ItemStack> stacks)
    {
        if (uiController != null)
            uiController.RefreshUI(stacks);
    }

    public void AddItem(InventoryItem item, int amount = 1)
    {
        playerInventory.AddItem(item, amount);
        // Inventory will trigger OnInventoryChanged automatically
    }

    public void RemoveItem(InventoryItem item, int amount = 1)
    {
        playerInventory.RemoveItem(item, amount);
        // Inventory will trigger OnInventoryChanged automatically
    }

    // public void ClearInventory() {
    //     inventory.Clear(); // Assuming you have a Clear() method
    //     // UI will update via event
    // }

    public void OpenInventoryUI()
    {
        uiController.gameObject.SetActive(true);
        uiController.RefreshUI(playerInventory.GetAllItems());
    }

    public void CloseInventoryUI()
    {
        uiController.gameObject.SetActive(false);
    }
}
