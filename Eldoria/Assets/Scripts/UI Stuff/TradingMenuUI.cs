using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradingMenuUI : MenuController, ICardHandler<ItemStack>, IMenuWithSource
{
    [SerializeField] private Transform traderItemsPanel;
    [SerializeField] private Transform playerItemsPanel;
    [SerializeField] private TMP_Text traderText;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private TextMeshProUGUI goldTransferAmountText;

    [SerializeField] private Button confirmButton;


    [SerializeField] private List<ItemStack> traderDisplayItems = new();
    [SerializeField] private List<ItemStack> playerDisplayItems = new();
    [SerializeField] private List<ItemStack> playerPendingPurchase = new();
    [SerializeField] private List<ItemStack> playerPendingSale = new();
    private InventoryManager traderInventory;
    [SerializeField] private InventoryManager playerInventory;
    private int goldTransferToTraderAmount = 0;

    void Awake()
    {
        confirmButton.onClick.AddListener(() =>
        {
            // add stuff to trader inventory
            foreach (ItemStack stack in playerPendingSale)
            {
                if (PlayerHas(stack))
                {
                    Debug.Log("attempting sale: " + stack.item + ", " + stack.quantity);
                    int tradeAmount = stack.quantity;
                    // sell item
                    playerInventory.RemoveItem(stack.item, tradeAmount);
                    traderInventory.AddItem(stack.item, tradeAmount);
                    GameManager.Instance.PlayerProfile.AddGold(stack.quantity * stack.item.baseCost);
                }
            }

            // add stuff to player inventory
            foreach (ItemStack stack in playerPendingPurchase)
            {
                if (TraderHas(stack))
                {
                    int tradeAmount = stack.quantity;
                    Debug.Log("attempting purchase: " + stack.item + ", " + stack.quantity);
                    // buy item
                    traderInventory.RemoveItem(stack.item, tradeAmount);
                    playerInventory.AddItem(stack.item, tradeAmount);
                    GameManager.Instance.PlayerProfile.AddGold(-stack.quantity * stack.item.baseCost);
                }
            }
            goldTransferToTraderAmount = 0;
            UpdateGoldTransferAmountText();
            playerPendingPurchase.Clear();
            playerPendingSale.Clear();
            GetTraderItems();
            GetPlayerItems();
            PopulateGrid(traderItemsPanel, traderDisplayItems);
            PopulateGrid(playerItemsPanel, playerDisplayItems);
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
        PopulateGrid(traderItemsPanel, traderDisplayItems);
        PopulateGrid(playerItemsPanel, playerDisplayItems);
        UpdateGoldTransferAmountText();
    }

    void GetTraderItems()
    {
        traderDisplayItems.Clear();
        foreach (ItemStack item in traderInventory.Inventory.GetAllItems())
        {
            traderDisplayItems.Add(item);
        }
    }

    void GetPlayerItems()
    {
        playerDisplayItems.Clear();
        foreach (ItemStack item in playerInventory.Inventory.GetAllItems())
        {
            playerDisplayItems.Add(item);
        }
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
        // Check if the clicked stack is in the player's display
        if (playerDisplayItems.Contains(stack))
        {
            // Remove one matching stack from pending purchase if it exists
            var match = playerPendingPurchase.FirstOrDefault(s => s.item == stack.item && s.quantity == stack.quantity);
            if (match != null) playerPendingPurchase.Remove(match);
            else playerPendingSale.Add(new ItemStack(stack.item, stack.quantity));
            UpdateGoldTransferAmount(-stack.quantity * stack.item.baseCost);

            // Move the stack visually
            playerDisplayItems.Remove(stack);
            traderDisplayItems.Add(new ItemStack(stack.item, stack.quantity));

            Debug.Log($"{stack.item.name}, {stack.quantity} was removed from player items and added to trader items");
        }
        // Check if the clicked stack is in the trader's display
        else if (traderDisplayItems.Contains(stack))
        {
            // Remove one matching stack from pending sale if it exists
            var match = playerPendingSale.FirstOrDefault(s => s.item == stack.item && s.quantity == stack.quantity);
            if (match != null) playerPendingSale.Remove(match);
            else playerPendingPurchase.Add(new ItemStack(stack.item, stack.quantity));
            UpdateGoldTransferAmount(stack.quantity * stack.item.baseCost);

            // Move the stack visually
            traderDisplayItems.Remove(stack);
            playerDisplayItems.Add(new ItemStack(stack.item, stack.quantity));

            Debug.Log($"{stack.item.name}, {stack.quantity} was removed from trader items and added to player items");
        }
        else
        {
            Debug.LogWarning("Clicked item not found in either display list");
        }

        // Refresh UI
        PopulateGrid(traderItemsPanel, traderDisplayItems);
        PopulateGrid(playerItemsPanel, playerDisplayItems);
        // if (playerDisplayItems.Contains(stack))
        // {
        //     if (playerPendingPurchase.Contains(stack)) playerPendingPurchase.Remove(stack);
        //     else playerPendingSale.Add(stack);
        //     playerDisplayItems.Remove(stack);
        //     traderDisplayItems.Add(stack);
        //     Debug.Log(stack.item + ", " + stack.quantity + " was removed from player items and added to trader items");
        // }
        // else if (traderDisplayItems.Contains(stack))
        // {
        //     if (playerPendingSale.Contains(stack)) playerPendingSale.Remove(stack);
        //     else playerPendingPurchase.Add(stack);
        //     traderDisplayItems.Remove(stack);
        //     playerDisplayItems.Add(stack);
        //     Debug.Log(stack.item + ", " + stack.quantity + " was removed from trader items and added to trader items");

        // }
        // else Debug.LogWarning("Something is wrong. Idk how this could have even happened");

        // // update grids
        // PopulateGrid(traderItemsPanel, traderDisplayItems);
        // PopulateGrid(playerItemsPanel, playerDisplayItems);
    }
    private void UpdateGoldTransferAmount(int amount)
    {
        goldTransferToTraderAmount += amount;
        UpdateGoldTransferAmountText();

        if (goldTransferToTraderAmount > 0)
        {
            // player is giving gold to trader
            if (GameManager.Instance.PlayerProfile.CanAfford(goldTransferToTraderAmount))
            {
                goldTransferAmountText.color = Color.white;
                confirmButton.interactable = true;
            }
            else
            {
                goldTransferAmountText.color = Color.red;
                confirmButton.interactable = false;
            }
        }
        else
        {
            // trader is giving gold to player
            goldTransferAmountText.color = Color.white;
            confirmButton.interactable = true;
        }
    }

    private void UpdateGoldTransferAmountText()
    {
        if (goldTransferToTraderAmount > 0)
        {
            goldTransferAmountText.text = "=>\n" + goldTransferToTraderAmount.ToString() + "\n=>";

        }
        else if (goldTransferToTraderAmount == 0)
        {
            goldTransferAmountText.text = "";
        }
        else if (goldTransferToTraderAmount < 0)
        {
            goldTransferAmountText.text = "<=\n" + (-goldTransferToTraderAmount).ToString() + "\n<=";
        }
    }

    bool PlayerHas(ItemStack stack) =>
    playerInventory.Inventory.GetAllItems().Any(s => s.item == stack.item && s.quantity == stack.quantity);
    bool TraderHas(ItemStack stack) =>
        traderInventory.Inventory.GetAllItems().Any(s => s.item == stack.item && s.quantity == stack.quantity);


}
