using System;
using System.Collections.Generic;
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
        UIManager.Instance.OpenWaitingMenu("wait");
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
            UIManager.Instance.OpenWaitingMenu("siege");
        }

    }

    public void EndSiege()
    {
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
            }

            else
            {
                Debug.Log("starting siege battle");
                CombatSimulator.StartSiegeBattle(transform.position, siegeAttacker.GetComponent<PartyPresence>().Lord.Faction, GetFaction(), this);

            }

            // siege is over
            siegeAttacker = null;
            siegeByPlayer = false;
        }
    }

    public void InitializeGarrison()
    {
        foreach (UnitData unitData in startingGarrisonUnits)
        {
            garrisonController.AddUnit(new UnitInstance(unitData));
        }
    }
}
