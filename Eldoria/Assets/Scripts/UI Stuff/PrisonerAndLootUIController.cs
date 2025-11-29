using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;

public class PrisonerAndLootUIController : MenuController, IMenuWithSource, ICardHandler<SoldierInstance>, ICardHandler<ItemStack>
{
    [SerializeField] private Transform potentialPrisonersGrid;
    [SerializeField] private Transform currentPrisonersGrid;
    [SerializeField] private Transform potentialLootGrid;
    [SerializeField] private Transform currentLootGrid;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private TextMeshProUGUI goldEarnedText;

    [SerializeField] private GameObject playerParty; // will provide current prisoners and current inventory
    private PartyController playerPartyController;
    private InventoryManager playerInventoryManager;


    [SerializeField] private Button confirmButton;
    private List<SoldierInstance> currentPrisonerList = new();
    private List<SoldierInstance> potentialPrisonerList = new();

    public List<ItemStack> currentLootList = new();
    public List<ItemStack> potentialLootList = new();

    private int goldEarned = 0;


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

            foreach (SoldierInstance prisoner in currentPrisonerList)
            {
                playerPartyController.AddPrisoner(prisoner);
            }

            GameManager.Instance.PlayerProfile.AddGold(goldEarned);


            // close menu
            UIManager.Instance.CloseAllMenus();
        });
    }


    public void OpenMenu(object source)
    {
        if (source is not PrisonerAndLootMenuContext)
        {
            Debug.LogWarning("PrisonerAndLootMenuContext expected as source");
            return;
        }
        PrisonerAndLootMenuContext ctx = source as PrisonerAndLootMenuContext;
        potentialLootList.Clear();
        potentialPrisonerList.Clear();
        currentLootList.Clear();
        currentPrisonerList.Clear();
        // populate prisoner list: 
        potentialPrisonerList.AddRange(ctx.potentialPrisoners);
        if (potentialPrisonerList == null)
        {
            Debug.LogWarning("something is wrong, potentialPrisonerList is null");
            return;
        }

        currentPrisonerList.AddRange(playerPartyController.Prisoners);


        // populate Loot lists: 
        currentLootList.AddRange(playerInventoryManager.GetAllItems());

        // set loot list
        potentialLootList = ctx.potentialLoot;
        // set gold earned
        SetGoldEarned(ctx.goldEarned);


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

    private void PopulatePrisonerGrid(Transform grid, List<SoldierInstance> units)
    {
        // clear the grid
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }

        // populate the grid
        foreach (SoldierInstance unit in units)
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

    public void OnCardClicked(SoldierInstance data)
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

    private void SetGoldEarned(int amount)
    {
        goldEarned = amount;
        goldEarnedText.text = "Gold Earned: " + goldEarned.ToString();
    }
}


public class PrisonerAndLootMenuContext
{
    public List<SoldierInstance> potentialPrisoners;
    public int goldEarned;
    public List<ItemStack> potentialLoot;

    public PrisonerAndLootMenuContext(List<SoldierInstance> potentialPrisoners, int goldEarned, List<ItemStack> potentialLoot)
    {
        this.potentialPrisoners = potentialPrisoners;
        this.goldEarned = goldEarned;
        this.potentialLoot = potentialLoot;
    }
}
