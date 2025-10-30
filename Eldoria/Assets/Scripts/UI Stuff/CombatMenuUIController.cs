using System.Collections.Generic;
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

    private PartyController attackingPartyController;
    private PartyController defendingPartyController;
    private PartyPresence attackingPartyPresence;
    private PartyPresence defendingPartyPresence;
    private bool isPlayerAttacking;

    public void Awake()
    {

    }

    public void OpenMenu(object source)
    {
        if (source is CombatMenuContext ctx)
        {
            isPlayerAttacking = ctx.isPlayerAttacking;
            if (isPlayerAttacking)
            {
                attackingText.text = "Player";
                defendingText.text = defendingPartyPresence.Lord.UnitName;
                attackingPartyController = playerPartyController;
                defendingPartyController = ctx.enemyParty;
                attackingPartyPresence = attackingPartyController.GetComponent<PartyPresence>();
                defendingPartyPresence = defendingPartyController.GetComponent<PartyPresence>();

            }
            else if (!isPlayerAttacking)
            {
                attackingText.text = attackingPartyPresence.Lord.UnitName;
                defendingText.text = "Player";
                defendingPartyController = playerPartyController;
                attackingPartyController = ctx.enemyParty;
                attackingPartyPresence = attackingPartyController.GetComponent<PartyPresence>();
                defendingPartyPresence = defendingPartyController.GetComponent<PartyPresence>();

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
            CombatResult result = CombatSimulator.SimulateBattle(attackingPartyController.PartyMembers, defendingPartyController.PartyMembers);

            // show results
            PopulateGrid(attackingUnitsGrid, result.AttackerParty);
            PopulateGrid(defendingUnitsGrid, result.DefenderParty);

            // apply results
            CombatOutcomeProcessor.ApplyCombatResult(result, attackingPartyController, defendingPartyController);

            confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                if ((result.Party1Wins && isPlayerAttacking) || (!result.Party1Wins && !isPlayerAttacking))
                {
                    HandlePlayerWin();
                }
                else
                {
                    // destruction of player party?
                }


            });


        });


        // populate grids
        PopulateGrid(attackingUnitsGrid, attackingPartyController.PartyMembers);
        PopulateGrid(defendingUnitsGrid, defendingPartyController.PartyMembers);


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

    private void HandlePlayerWin()
    {
        List<UnitInstance> prisoners;
        if (isPlayerAttacking)
        {
            prisoners = CombatOutcomeProcessor.ReturnPrisoners(defendingPartyController);
        }
        else
        {
            prisoners = CombatOutcomeProcessor.ReturnPrisoners(attackingPartyController);
        }

        // progress to prisoner allotment / loot screen
        MainMenuController menu = gameObject.transform.parent?.GetComponent<MainMenuController>();
        if (menu == null)
        {
            Debug.LogWarning("Main menu not found");
            return;
        }

        // destroy the enemy party? 

        Destroy(defendingPartyController.gameObject);
        Debug.Log("attempting party destruction");


        menu.OpenSubMenu("prisoners", prisoners);



    }


}


public class CombatMenuContext
{
    public PartyController enemyParty;
    public bool isPlayerAttacking;

    public CombatMenuContext(PartyController enemyParty, bool isPlayerAttacking)
    {
        this.enemyParty = enemyParty;
        this.isPlayerAttacking = isPlayerAttacking;
    }
}

