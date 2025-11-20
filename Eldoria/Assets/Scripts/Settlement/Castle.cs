using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Castle : Settlement, IHasBoundVillages, ISiegable
{

    // garrison
    private PartyController garrisonController;

    public PartyController GarrisonController => garrisonController;

    [SerializeField] private int defenseBonus = 20;

    // bound villages

    [SerializeField] private List<Village> boundVillages;

    [Header("Garrison")]
    [SerializeField] private List<UnitData> startingGarrisonUnits;

    [SerializeField] private List<PartyPresence> parties;
    public List<PartyPresence> Parties => parties;

    public List<Village> BoundVillages => boundVillages;

    // siege
    private int siegeTicksRemaining;
    private PartyController siegeAttacker;
    private bool siegeByPlayer;

    public bool isUnderSiege => siegeTicksRemaining > 0;

    void Awake()
    {
        garrisonController = GetComponent<PartyController>();
        if (garrisonController == null) Debug.LogWarning("Expected party controller");
    }


    public override void Start()
    {
        base.Start();
        ApplyVisuals();
        InitializeGarrison();
    }

    public override void ApplyVisuals()
    {
        SpriteRenderer spriteRenderer = transform.Find("LeftFlag").GetComponent<SpriteRenderer>();
        spriteRenderer.color = GetFaction().factionColor;
        spriteRenderer = transform.Find("RightFlag").GetComponent<SpriteRenderer>();
        spriteRenderer.color = GetFaction().factionColor;


        TextMeshProUGUI castleNameText = transform.Find("Canvas/CastleNameText").GetComponent<TextMeshProUGUI>();
        castleNameText.text = gameObject.name;
    }


    public override List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new();

        if (FactionsManager.Instance.AreEnemies(GetFaction(), FactionsManager.Instance.GetFactionByName("Player")))
        {
            options.Add(new InteractionOption("Besiege Castle", () =>
            {
                PartyController controller = GameManager.Instance.player.GetComponent<PartyController>();
                if (controller == null)
                {
                    Debug.Log("No player party controller found");
                    return;
                }
                StartSiege(controller, true);
            }
                ));
        }

        else if (TerritoryManager.Instance.GetLordOf(this) == GameManager.Instance.PlayerProfile)
        {
            // allow upgrades TODO: make an upgrade menu
            options.Add(new InteractionOption("Upgrade Defences", () =>
            {

                FortifyCastle();

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

        else
        {
            options.Add(new InteractionOption("Sell Prisoners", () =>
           {
               UIManager.Instance.OpenSubMenu("prisoner_sell", null);
           }));

            options.Add(new InteractionOption("Wait Inside", () => WaitInsideCastle()));
        }

        options.Add(new InteractionOption("Leave", () => { UIManager.Instance.CloseAllMenus(); }));
        return options;
    }

    private void WaitInsideCastle()
    {
        // open wait menu
        AddParty(GameManager.Instance.player.GetComponent<PartyPresence>());
        UIManager.Instance.OpenWaitingMenu(new WaitingMenuContext(false, this));
        GameManager.Instance.player.GetComponent<PartyPresence>().WaitInFief();
    }

    private void BesiegeCastle()
    {
        // open siege menu

        // pause any production

        // start timer for battlescreen to open
    }

    private void FortifyCastle()
    {
        defenseBonus += 5;
    }

    public override void Interact()
    {
        Debug.Log("Attempting interaction with castle");
    }

    public void StartSiege(PartyController attacker, bool isPlayer)
    {
        if (isUnderSiege)
        {
            Debug.Log("Apparently a besieged castle is under siege again...");
            return;
        }
        Debug.Log($"{name} is besieged by {attacker.name}");
        FactionsManager.Instance.GetWarManager(GetFaction()).NotifySettlementUnderSiege(this);

        siegeAttacker = attacker;
        siegeByPlayer = isPlayer;
        siegeTicksRemaining = GetSiegeTicks();

        TickManager.Instance.OnTick += HandleTick;

        if (siegeByPlayer)
        {
            UIManager.Instance.OpenWaitingMenu(new WaitingMenuContext(true, this));
        }

    }

    public void EndSiege()
    {
        Debug.Log($"{name} siege ended.");
        siegeAttacker = null;
        siegeByPlayer = false;
        siegeTicksRemaining = 0;

        TickManager.Instance.OnTick -= HandleTick;
    }
    private int GetSiegeTicks()
    {
        return (int)Math.Round((double)prosperity / 100);
    }

    public void HandleTick(int i)
    {
        if (!isUnderSiege) return;

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
                CombatSimulator.StartSiegeBattle(transform.position, siegeAttacker.GetComponent<PartyPresence>().Lord.Faction, GetFaction(), this, true);
                EndSiege();
            }
            else if (parties.Contains(GameManager.Instance.player.GetComponent<PartyPresence>()))
            {
                UIManager.Instance.CloseAllMenus();
                CombatSimulator.StartSiegeBattle(transform.position, siegeAttacker.GetComponent<PartyPresence>().Lord.Faction, GetFaction(), this, false);
                EndSiege();
            }

            else
            {
                Debug.Log("starting siege battle");
                siegeAttacker.TryGetComponent<LordNPCStateMachine>(out var stateMachine);
                CombatSimulator.StartSiegeBattle(transform.position, siegeAttacker.GetComponent<PartyPresence>().Lord.Faction, GetFaction(), this);
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
            if (FactionsManager.Instance.AreAllied(GetFaction(), p.Lord.Faction))
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

    public void InitializeGarrison()
    {
        foreach (UnitData unitData in startingGarrisonUnits)
        {
            garrisonController.AddUnit(new UnitInstance(unitData));
        }
    }
}
