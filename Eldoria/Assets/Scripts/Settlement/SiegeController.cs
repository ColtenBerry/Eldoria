using UnityEngine;
using System;
using System.Collections.Generic;

public class SiegeController : MonoBehaviour, ISiegable, IInteractable
{
    private int siegeTicksRemaining;
    private PartyPresence siegeAttacker;
    private bool siegeByPlayer;

    public bool IsUnderSiege => siegeTicksRemaining > 0;

    private PartyController garrisonController;

    public PartyController GarrisonController => garrisonController;

    [SerializeField] private List<UnitData> startingGarrisonUnits;


    [SerializeField] private int defenseBonus = 20;

    [SerializeField] private List<PartyPresence> parties;
    public List<PartyPresence> Parties => parties;


    private Settlement settlement;
    public Settlement Settlement => settlement;



    void Awake()
    {
        settlement = GetComponent<Settlement>();
        if (settlement == null)
            Debug.LogError("SiegeController requires a Settlement component");

        garrisonController = GetComponent<PartyController>();
        if (settlement == null)
            Debug.LogError("SiegeController requires a partycontroller component");
    }

    public void Start()
    {
        InitializeGarrison();
    }

    public void InitializeGarrison()
    {
        foreach (UnitData unitData in startingGarrisonUnits)
        {
            garrisonController.AddUnit(new UnitInstance(unitData));
        }
        garrisonController.SetIsHealing(true);
    }


    public void StartSiege(PartyPresence attacker, bool isPlayer)
    {
        if (IsUnderSiege)
        {
            Debug.Log($"{settlement.name} is already under siege.");
            return;
        }
        FactionsManager.Instance.GetWarManager(Settlement.GetFaction()).NotifySettlementUnderSiege(Settlement);
        siegeAttacker = attacker;
        siegeByPlayer = isPlayer;
        siegeTicksRemaining = GetSiegeTicks();

        TickManager.Instance.OnTick += HandleTick;

        Debug.Log($"{settlement.name} is besieged by {attacker.name}");

        if (siegeByPlayer)
        {
            UIManager.Instance.OpenWaitingMenu(new WaitingMenuContext(true, this));
        }
    }

    public void EndSiege()
    {
        if (siegeAttacker == null) return;
        Debug.Log($"{settlement.name} siege ended.");
        FactionsManager.Instance.GetWarManager(siegeAttacker.Lord.Faction).CancelDefense(Settlement);

        siegeAttacker = null;
        siegeByPlayer = false;
        siegeTicksRemaining = 0;

        TickManager.Instance.OnTick -= HandleTick;

    }

    public int GetSiegeTicks()
    {
        return Mathf.Max(1, settlement.GetProsperity() / 100);
    }

    public void HandleTick(int i)
    {
        HandleSiegeTicks();
    }


    private void HandleSiegeTicks()
    {
        if (!IsUnderSiege) return;

        siegeTicksRemaining--;

        if (siegeByPlayer)
        {
            // UIManager.Instance.UPdateSiegeMenu(siegeTicksRemaining);
        }

        if (siegeTicksRemaining <= 0)
        {
            // begin battle
            if (siegeByPlayer)
            {
                UIManager.Instance.CloseAllMenus();
                CombatSimulator.StartSiegeBattle(transform.position, siegeAttacker.GetComponent<PartyPresence>().Lord.Faction, settlement.GetFaction(), this, true);
                EndSiege();
            }
            else if (parties.Contains(GameManager.Instance.player.GetComponent<PartyPresence>()))
            {
                siegeAttacker.TryGetComponent<LordNPCStateMachine>(out var stateMachine);

                UIManager.Instance.CloseAllMenus();
                CombatSimulator.StartSiegeBattle(transform.position, siegeAttacker.GetComponent<PartyPresence>().Lord.Faction, settlement.GetFaction(), this, false);
                stateMachine.EndSiege(this);
                EndSiege();
            }

            else
            {
                Debug.Log("starting siege battle");
                siegeAttacker.TryGetComponent<LordNPCStateMachine>(out var stateMachine);
                CombatSimulator.StartSiegeBattle(transform.position, siegeAttacker.GetComponent<PartyPresence>().Lord.Faction, settlement.GetFaction(), this);
                stateMachine.EndSiege(this);
                EndSiege();

            }

            // siege is over
            siegeAttacker = null;
            siegeByPlayer = false;
        }

    }

    public void AddParty(PartyPresence party)
    {
        if (parties.Contains(party)) return;
        parties.Add(party);
    }

    public void RemoveParty(PartyPresence party)
    {
        if (!parties.Contains(party)) return;

        parties.Remove(party);
    }

    public List<PartyController> GetAlliedPartyControllers()
    {
        List<PartyController> lst = new();
        foreach (PartyPresence p in parties)
        {
            if (FactionsManager.Instance.AreAllied(settlement.GetFaction(), p.Lord.Faction))
            {
                PartyController controller = p.GetComponent<PartyController>();
                if (controller == null)
                {
                    Debug.LogError("expected controller in castle defender list");
                }
                lst.Add(controller);
            }
        }
        return lst;
    }

    public void Interact()
    {
    }

    public List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new();

        if (FactionsManager.Instance.AreEnemies(settlement.GetFaction(), FactionsManager.Instance.GetFactionByName("Player")))
        {
            options.Add(new InteractionOption("Besiege Castle", () =>
            {
                PartyPresence controller = GameManager.Instance.player.GetComponent<PartyPresence>();
                if (controller == null)
                {
                    Debug.Log("No player party controller found");
                    return;
                }
                StartSiege(controller, true);
            }

                ));
        }
        if (TerritoryManager.Instance.GetLordOf(settlement) == GameManager.Instance.PlayerProfile)
        {
            // allow upgrades TODO: make an upgrade menu
            options.Add(new InteractionOption("Upgrade Defences", () =>
            {

                //FortifyCastle();

            }));

            options.Add(new InteractionOption("Manage Garrison", () =>
            {
                UIManager.Instance.OpenSubMenu("garrison", new PartyTransferMenuContext(garrisonController, GameManager.Instance.player.GetComponent<PartyController>()));
            }));

            options.Add(new InteractionOption("Sell Prisoners", () =>
            {
                UIManager.Instance.OpenSubMenu("prisoner_sell", null);
            }));

            options.Add(new InteractionOption("Wait Inside", () => WaitInsideCastle()));
        }

        return options;
    }

    private void WaitInsideCastle()
    {
        // open wait menu
        AddParty(GameManager.Instance.player.GetComponent<PartyPresence>());
        GameManager.Instance.player.GetComponent<PartyPresence>().WaitInFief();
        UIManager.Instance.OpenWaitingMenu(new WaitingMenuContext(false, this));
    }

    public int GetTotalDefenderStrength()
    {
        int strength = 0;
        foreach (UnitInstance unit in garrisonController.PartyMembers)
        {
            strength += ((unit.Attack + unit.Defence) * unit.Health);
        }
        foreach (PartyPresence presence in parties)
        {
            strength += presence.GetStrengthEstimate();
        }


        return strength;
    }

}
