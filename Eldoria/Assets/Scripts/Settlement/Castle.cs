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

    }

    private void ApplyVisuals()
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
            options.Add(new InteractionOption("Upgrade Defences", () => FortifyCastle()));

            options.Add(new InteractionOption("Wait Inside", () => WaitInsideCastle()));
        }

        else
        {
            options.Add(new InteractionOption("Wait Inside", () => WaitInsideCastle()));
        }

        options.Add(new InteractionOption("Leave", () => { InputGate.OnMenuClosed?.Invoke(); }));
        return options;
    }

    private void WaitInsideCastle()
    {
        // open wait menu
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

        siegeAttacker = attacker;
        siegeByPlayer = isPlayer;
        siegeTicksRemaining = GetSiegeTicks();

        TickManager.Instance.OnTick += HandleTick;

        if (siegeByPlayer)
        {
            UIManager.Instance.OpenWaitingMenu("siege");
        }

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
                UIManager.Instance.CloseWaitingMenu();
                CombatSimulator.InitiateCombat(garrisonController, true);
            }

            else
            {
                CombatSimulator.InitiateCombat(siegeAttacker, garrisonController);
            }

            // siege is over
            siegeAttacker = null;
            siegeByPlayer = false;
        }
    }
}
