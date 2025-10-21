using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatMenuUIController : MenuController, IMenuWithSource, ICardHandler<UnitInstance>
{
    [SerializeField] private Transform playerUnitsGrid;
    [SerializeField] private Transform enemyUnitsGrid;
    [SerializeField] private TMP_Text enemyText;
    [SerializeField] private GameObject unitPrefab;

    [SerializeField] private Button confirmButton;

    private PartyController enemyPartyController;

    [SerializeField] private PartyController playerPartyController;



    public void Awake()
    {
        confirmButton.onClick.AddListener(() =>
        {
            // do stuff
            CombatResult result = CombatSimulator.SimulateBattle(playerPartyController.PartyMembers, enemyPartyController.PartyMembers);

            // show results
            PopulateGrid(playerUnitsGrid, result.AttackerParty);
            PopulateGrid(enemyUnitsGrid, result.DefenderParty);

            // apply results
            CombatOutCombeProcessor.ApplyCombatResult(result, playerPartyController, enemyPartyController);

            confirmButton.GetComponent<Text>().text = "Continue";
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                // progress to prisoner allotment / loot screen


            });


        });

    }

    public void OpenMenu(object source)
    {
        var component = source as Component;
        if (component == null)
        {
            Debug.LogWarning("Combat Menu expected a Component-based interface");
            return;
        }

        enemyPartyController = component.GetComponent<PartyController>();
        if (enemyPartyController == null)
        {
            Debug.LogWarning("PartyController not found on source GameObject");
            return;
        }

        // populate grids
        PopulateGrid(playerUnitsGrid, playerPartyController.PartyMembers);
        PopulateGrid(enemyUnitsGrid, enemyPartyController.PartyMembers);


    }

    private void PopulateGrid(Transform grid, List<UnitInstance> units)
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

    public void OnCardClicked(UnitInstance data)
    {
        // do nothing
    }


}
