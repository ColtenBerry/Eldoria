using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatMenuUIController : MenuController, IMenuWithSource, ICardHandler<UnitInstance>
{
    [SerializeField] private Transform attackingUnitsGrid;
    [SerializeField] private Transform defendingUnitsGrid;
    [SerializeField] private TMP_Text attackingText;
    [SerializeField] private TMP_Text defendingText;
    [SerializeField] private GameObject unitPrefab;

    [SerializeField] private Button confirmButton;
    [SerializeField] private PartyController playerPartyController;

    // private PartyController enemyPartyController;
    // private PartyPresence enemyPartyPresence;

    // [SerializeField] private PartyController playerPartyController;

    private List<PartyController> attackingPartyControllers;
    private List<PartyController> defendingPartyControllers;
    private bool isPlayerAttacking;
    private bool isSiegeBattle;
    Settlement fief;

    public void Awake()
    {

    }

    public void OpenMenu(object source)
    {
        if (source is CombatMenuContext ctx)
        {
            isPlayerAttacking = ctx.isPlayerAttacking;
            isSiegeBattle = ctx.isSiegeBattle;
            fief = ctx.fiefUnderSiege;
            if (isPlayerAttacking)
            {
                attackingPartyControllers = new List<PartyController>
                {
                    playerPartyController
                };
                attackingPartyControllers.AddRange(ctx.friendlyParties);
                defendingPartyControllers = ctx.enemyParties;
                attackingText.text = "Player";
                defendingText.text = ctx.enemyName;

            }
            else if (!isPlayerAttacking)
            {
                defendingPartyControllers = new List<PartyController>
                {
                    playerPartyController
                };
                defendingPartyControllers.AddRange(ctx.friendlyParties);
                attackingPartyControllers = ctx.enemyParties;
                attackingText.text = ctx.enemyName;
                defendingText.text = "Player";

            }

        }
        else
        {
            Debug.LogWarning("Expected CombatMenuContext");
        }




        confirmButton.onClick.RemoveAllListeners();

        confirmButton.onClick.AddListener(() =>
        {
            // do stuff
            CombatResult result = CombatSimulator.SimulateBattle(attackingPartyControllers, defendingPartyControllers);

            // show results
            PopulateGrid(attackingUnitsGrid, result.AttackerUnits);
            PopulateGrid(defendingUnitsGrid, result.DefenderUnits);


            confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                CombatOutcomeProcessor.ProcessPlayerBattleResult(result, attackingPartyControllers, defendingPartyControllers, isPlayerAttacking, isSiegeBattle, fief);

            });


        });


        // populate grids


        // Flatten all units from each party controller
        List<UnitInstance> attackingUnits = attackingPartyControllers
            .SelectMany(party => party.PartyMembers)
            .ToList();

        List<UnitInstance> defendingUnits = defendingPartyControllers
            .SelectMany(party => party.PartyMembers)
            .ToList();

        // Populate the grids with the full unit lists
        PopulateGrid(attackingUnitsGrid, attackingUnits);
        PopulateGrid(defendingUnitsGrid, defendingUnits);

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


public class CombatMenuContext
{
    public List<PartyController> friendlyParties;
    public List<PartyController> enemyParties;
    public bool isSiegeBattle;
    public bool isPlayerAttacking;
    public Settlement fiefUnderSiege;
    public string enemyName;

    public CombatMenuContext(List<PartyController> friendlyParties, List<PartyController> enemyParties, bool isPlayerAttacking, string enemyName, bool isSiegeBattle = false, Settlement fiefUnderSiege = null)
    {
        this.friendlyParties = friendlyParties;
        this.enemyParties = enemyParties;
        this.isPlayerAttacking = isPlayerAttacking;
        this.enemyName = enemyName;
        this.isSiegeBattle = isSiegeBattle;
        this.fiefUnderSiege = fiefUnderSiege;
    }
}

