using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;

public class PrisonerAndLootUIController : MenuController, IMenuWithSource, ICardHandler<UnitInstance>, ICardHandler<ItemStack>
{
    [SerializeField] private Transform potentialPrisonersGrid;
    [SerializeField] private Transform currentPrisonersGrid;
    [SerializeField] private Transform potentialLootGrid;
    [SerializeField] private Transform currentLootGrid;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private GameObject playerParty; // will provide current prisoners and current inventory
    private PartyController playerPartyController;
    private InventoryManager playerInventoryManager;


    [SerializeField] private Button confirmButton;
    private List<UnitInstance> currentPrisonerList = new();
    private List<UnitInstance> potentialPrisonerList = new();

    public List<ItemStack> currentLootList = new();
    public List<ItemStack> potentialLootList = new();


    public void Awake()
    {
        playerPartyController = playerParty.GetComponent<PartyController>();
        playerInventoryManager = playerParty.GetComponent<InventoryManager>();
        if (playerPartyController == null) Debug.LogWarning("Player Party Controller not found");
        if (playerInventoryManager == null) Debug.LogWarning("Player Inventory Manager is not found");
        confirmButton.onClick.AddListener(() =>
        {
            // update player inventory
            playerInventoryManager.ClearAllItems();

            foreach (ItemStack item in currentLootList)
            {
                playerInventoryManager.AddItem(item.item, item.quantity);
            }

            // update player prisoners
            playerPartyController.ClearPrisoners();

            foreach (UnitInstance prisoner in currentPrisonerList)
            {
                playerPartyController.AddPrisoner(prisoner);
            }

            // close menu
            InputGate.OnMenuClosed?.Invoke();
        });
    }


    public void OpenMenu(object source)
    {
        potentialLootList.Clear();
        potentialPrisonerList.Clear();
        currentLootList.Clear();
        currentPrisonerList.Clear();
        // populate prisoner list: 
        potentialPrisonerList.AddRange(source as List<UnitInstance>);
        if (potentialPrisonerList == null)
        {
            Debug.LogWarning("OpenMenu called with invalid source type.");
            return;
        }

        currentPrisonerList.AddRange(playerPartyController.Prisoners);


        // populate Loot lists: 
        currentLootList.AddRange(playerInventoryManager.GetAllItems());

        potentialLootList = new();


        PopulatePrisonerGrids();
        PopulateLootGrids();
    }

    private void PopulatePrisonerGrids()
    {
        PopulatePrisonerGrid(potentialPrisonersGrid, potentialPrisonerList);
        PopulatePrisonerGrid(currentPrisonersGrid, currentPrisonerList);

    }

    private void PopulateLootGrids()
    {
        PopulateLootGrid(potentialLootGrid, potentialLootList);
        PopulateLootGrid(currentLootGrid, currentLootList);
    }

    private void PopulatePrisonerGrid(Transform grid, List<UnitInstance> units)
    {
        // clear the grid
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }

        // populate the grid
        foreach (UnitInstance unit in units)
        {
            var thing = Instantiate(unitPrefab, grid);
            // setup thing
            thing.GetComponent<PartyMemberUI>().Setup(unit, this);

        }
    }
    private void PopulateLootGrid(Transform grid, List<ItemStack> itemStacks)
    {
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }

        // populate the grid
        foreach (ItemStack stack in itemStacks)
        {
            var thing = Instantiate(itemPrefab, grid);
            // setup thing
            thing.GetComponent<ItemSlotUI>().Setup(stack, this);

        }

    }

    public void OnCardClicked(ItemStack data)
    {
        if (currentLootList.Contains(data))
        {
            currentLootList.Remove(data);
            potentialLootList.Add(data);
        }
        else if (potentialLootList.Contains(data))
        {
            potentialLootList.Remove(data);
            currentLootList.Add(data);
        }

        PopulateLootGrids();

    }

    public void OnCardClicked(UnitInstance data)
    {
        if (currentPrisonerList.Contains(data))
        {
            currentPrisonerList.Remove(data);
            potentialPrisonerList.Add(data);
        }
        else if (potentialPrisonerList.Contains(data))
        {
            potentialPrisonerList.Remove(data);
            currentPrisonerList.Add(data);
        }

        PopulatePrisonerGrids();
    }
}
