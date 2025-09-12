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
        // if (playerInventory == null) return;
        playerInventory.AddItem(item, amount);
        // Inventory will trigger OnInventoryChanged automatically
    }

    public void RemoveItem(InventoryItem item, int amount = 1)
    {
        if (playerInventory == null) return;
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


    // TODO: 
    /*
        - Have this listen for a button to be pressed (to drop an item). At which point, the item will be dropped
            - I am assuming that we can pass the item through the action. 
        - Our current action, OnInventoryChanged, will not function for other inventories. Inventory should be reusable for each 
        - Perhaps this class can be generalized to be used for any inventory? and can be initialized on the player? and each other inventory?
            - This can just be a script on anything that may find use for it. probably just settlments for now. and the player, obviously 

        - Are changes even needed? I can use some other logic to keep track of inventories... this doesn't have to be an in depth game. Just sell food
        some random weapons, armor, etc? maybe lets wait till later. 

    */
}
