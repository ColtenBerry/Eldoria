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
                attackingPartyController = playerPartyController;
                defendingPartyController = ctx.enemyParty;
                attackingText.text = "Player";
                defendingText.text = ctx.enemyName;

            }
            else if (!isPlayerAttacking)
            {
                defendingPartyController = playerPartyController;
                attackingPartyController = ctx.enemyParty;
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
                //player wins
                if ((result.Party1Wins && isPlayerAttacking) || (!result.Party1Wins && !isPlayerAttacking))
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
                    UIManager.Instance.OpenSubMenu("prisoners", prisoners);

                    if (isSiegeBattle)
                    {
                        if (isPlayerAttacking)
                        {
                            TerritoryManager.Instance.RegisterOwnership(fief, GameManager.Instance.PlayerProfile);

                            // later on enter the territory
                        }
                        // player is not attacking, but player won. so attackers are destroyed  
                        else
                        {
                            Destroy(attackingPartyController.gameObject);
                        }
                    }
                    // not a siege battle
                    else
                    {
                        if (isPlayerAttacking) Destroy(defendingPartyController.gameObject);
                        else Destroy(attackingPartyController.gameObject);
                    }
                }
                // player loses
                else
                {
                    if (isSiegeBattle)
                    {
                        // owner of settlement loses the settlement
                        PartyPresence p = attackingPartyController.GetComponent<PartyPresence>();
                        if (p == null)
                        {
                            Debug.LogWarning("attacker party presence is null");
                        }
                        LordProfile lordProfile = LordRegistry.Instance.GetLordByName(p.Lord.Lord.UnitName);
                        TerritoryManager.Instance.RegisterOwnership(fief, lordProfile);
                    }
                    // player is captured
                    //idk how to do this now

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


}


public class CombatMenuContext
{
    public PartyController enemyParty;
    public bool isSiegeBattle;
    public bool isPlayerAttacking;
    public Settlement fiefUnderSiege;
    public string enemyName;

    public CombatMenuContext(PartyController enemyParty, bool isPlayerAttacking, string enemyName, bool isSiegeBattle = false, Settlement fiefUnderSiege = null)
    {
        this.enemyParty = enemyParty;
        this.isPlayerAttacking = isPlayerAttacking;
        this.enemyName = enemyName;
        this.isSiegeBattle = isSiegeBattle;
        this.fiefUnderSiege = fiefUnderSiege;
    }
}

