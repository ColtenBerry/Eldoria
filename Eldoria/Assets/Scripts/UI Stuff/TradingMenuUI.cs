using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TradingMenuUI : MonoBehaviour, ICardHandler<ItemStack>, IMenuWithSource
{
    [SerializeField] private Transform traderItemsPanel;
    [SerializeField] private Transform playerItemsPanel;
    [SerializeField] private TMP_Text traderText;
    [SerializeField] private GameObject itemPrefab;


    private List<ItemStack> traderItems;
    private List<ItemStack> playerItems;
    private InventoryManager traderInventory;
    [SerializeField] private Inventory playerInventory;

    void Awake()
    {

    }
    /*
        I would love to make IMenuWithSource<Inventory>. That way i can know exactly what i am getting here and 
        I don't have to go through the whole process of getting the Inventory script. But then I have to pass in the correct
        source when i use my interaction menu to make my main menu open the correct submenu. 
        How does the interaction menu know the source? Only if the interaction option knows the source
        How does the interaction option know the source? only if there is some Source object that is defined when constructed
        This can be done when defining interaction options


        Nope, Unity does not support the necessary opterations for this. 
    */

    public void OpenMenu(object source)
    {
        traderItems.Clear();
        var component = source as Component;
        if (component == null)
        {
            Debug.LogWarning("Trader Inventory expected a Component-based interface");
            return;
        }

        // Step 2: Get the TraderItems from the same GameObject
        traderInventory = component.GetComponent<InventoryManager>();
        if (traderInventory == null)
        {
            Debug.LogWarning("TraderInventory not found on source GameObject");
            return;
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
