using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatMenuUIController : MenuController, IMenuWithSource, ICardHandler<SoldierInstance>, ICardHandler<CharacterInstance>
{
    [SerializeField] private Transform attackingUnitsGrid;
    [SerializeField] private Transform defendingUnitsGrid;
    [SerializeField] private TMP_Text attackingText;
    [SerializeField] private TMP_Text defendingText;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private CharacterUI characterPrefab;


    [SerializeField] private Button confirmButton;
    [SerializeField] private PartyController playerPartyController;

    // private PartyController enemyPartyController;
    // private PartyPresence enemyPartyPresence;

    // [SerializeField] private PartyController playerPartyController;

    private List<PartyController> attackingPartyControllers;
    private List<PartyController> defendingPartyControllers;
    private List<CharacterInstance> attackingLords;
    private List<CharacterInstance> defendingLords;
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
            attackingPartyControllers = ctx.attackingParties;
            defendingPartyControllers = ctx.defendingParties;
            attackingText.text = ctx.attackingParties.FirstOrDefault().name;
            defendingText.text = ctx.defendingParties.FirstOrDefault().name;
            attackingLords = ctx.attackingLords;
            defendingLords = ctx.defendingLords;

        }
        else
        {
            Debug.LogWarning("Expected CombatMenuContext");
        }




        confirmButton.onClick.RemoveAllListeners();

        confirmButton.onClick.AddListener(() =>
        {
            // do stuff
            CombatResult result = CombatSimulator.SimulateBattle(attackingPartyControllers, defendingPartyControllers, attackingLords, defendingLords);

            // show results
            PopulateGrid(attackingUnitsGrid, result.AttackerUnits);
            PopulateGrid(defendingUnitsGrid, result.DefenderUnits);


            confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                CombatOutcomeProcessor.ProcessPlayerBattleResult(result, attackingPartyControllers, defendingPartyControllers, attackingLords, defendingLords, isPlayerAttacking, isSiegeBattle, fief);

            });


        });


        // populate grids


        // Flatten all units from each party controller
        List<UnitInstance> attackingUnits = new();
        attackingUnits.AddRange(attackingLords);
        attackingUnits.AddRange(
        attackingPartyControllers
            .SelectMany(party => party.PartyMembers)
            .ToList());

        List<UnitInstance> defendingUnits = new();
        defendingUnits.AddRange(defendingLords);
        defendingUnits.AddRange(
        defendingPartyControllers
            .SelectMany(party => party.PartyMembers)
            .ToList());

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
            if (unit is SoldierInstance soldier)
            {
                var thing = Instantiate(unitPrefab, grid);
                // setup thing
                thing.GetComponent<PartyMemberUI>().Setup(soldier, this);
            }
            else if (unit is CharacterInstance character)
            {
                CharacterUI characterUI = Instantiate(characterPrefab, grid);
                characterUI.Setup(character, this);
            }
            else
            {
                Debug.LogWarning("IDK what happened. not soldier or character.");
            }

        }
    }

    public void OnCardClicked(SoldierInstance data)
    {
        // do nothing
    }

    public void OnCardClicked(CharacterInstance data)
    {
        // do nothing
    }
}


public class CombatMenuContext
{
    public List<PartyController> attackingParties;
    public List<PartyController> defendingParties;
    public List<CharacterInstance> attackingLords;
    public List<CharacterInstance> defendingLords;
    public bool isSiegeBattle;
    public bool isPlayerAttacking;
    public Settlement fiefUnderSiege;
    public string enemyName;

    public CombatMenuContext(List<PartyController> attackingParties, List<PartyController> defendingParties, List<CharacterInstance> attackingLords, List<CharacterInstance> defendingLords, bool isPlayerAttacking, string enemyName, bool isSiegeBattle = false, Settlement fiefUnderSiege = null)
    {
        this.attackingParties = attackingParties;
        this.defendingParties = defendingParties;
        this.attackingLords = attackingLords;
        this.defendingLords = defendingLords;
        this.isPlayerAttacking = isPlayerAttacking;
        this.enemyName = enemyName;
        this.isSiegeBattle = isSiegeBattle;
        this.fiefUnderSiege = fiefUnderSiege;
    }
}

