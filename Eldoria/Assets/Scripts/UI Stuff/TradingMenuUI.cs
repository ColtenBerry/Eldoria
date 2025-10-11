using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradingMenuUI : MenuController, ICardHandler<ItemStack>, IMenuWithSource
{
    [SerializeField] private Transform traderItemsPanel;
    [SerializeField] private Transform playerItemsPanel;
    [SerializeField] private TMP_Text traderText;
    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private Button confirmButton;


    private List<ItemStack> traderItems = new();
    private List<ItemStack> playerItems = new();
    private InventoryManager traderInventory;
    [SerializeField] private InventoryManager playerInventory;

    void Awake()
    {
        confirmButton.onClick.AddListener(() =>
        {
            // add stuff to trader inventory
            foreach (ItemStack stack in traderItems)
            {
                if (playerInventory.Inventory.GetAllItems().Contains(stack))
                {
                    // sell item
                    playerInventory.RemoveItem(stack.item, stack.quantity);
                    traderInventory.AddItem(stack.item, stack.quantity);
                }
            }

            // add stuff to player inventory
            foreach (ItemStack stack in playerItems)
            {
                if (traderInventory.Inventory.GetAllItems().Contains(stack))
                {
                    // buy item
                    traderInventory.RemoveItem(stack.item, stack.quantity);
                    playerInventory.AddItem(stack.item, stack.quantity);
                }
            }
        });
    }

    public void OpenMenu(object source)
    {
        var component = source as Component;
        if (component == null)
        {
            Debug.LogWarning("Trader Inventory expected a Component-based interface");
            return;
        }

        traderInventory = component.GetComponent<InventoryManager>();
        if (traderInventory == null)
        {
            Debug.LogWarning("TraderInventory not found on source GameObject");
            return;
        }

        // get lists
        GetTraderItems();
        GetPlayerItems();
        PopulateGrid(traderItemsPanel, traderItems);
        PopulateGrid(playerItemsPanel, playerItems);
    }

    void GetTraderItems()
    {
        traderItems.Clear();
        traderItems = traderInventory.Inventory.GetAllItems();
    }

    void GetPlayerItems()
    {
        playerItems.Clear();
        playerItems = playerInventory.Inventory.GetAllItems();
    }

    public void PopulateGrid(Transform grid, List<ItemStack> list)
    {
        // clear the grid
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }

        // populate the grid
        foreach (ItemStack stack in list)
        {
            var thing = Instantiate(itemPrefab, grid);
            // setup thing
            thing.GetComponent<ItemSlotUI>().Setup(stack, this);

        }
    }

    public void OnCardClicked(ItemStack stack)
    {
        if (playerItems.Contains(stack))
        {
            playerItems.Remove(stack);
            traderItems.Add(stack);
        }
        else if (traderItems.Contains(stack))
        {
            traderItems.Remove(stack);
            playerItems.Add(stack);
        }
        else Debug.LogWarning("Something is wrong. Idk how this could have even happened");

        // update grids
        PopulateGrid(traderItemsPanel, traderItems);
        PopulateGrid(playerItemsPanel, playerItems);
    }

}
