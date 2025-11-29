using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SiegeController))]
public abstract class SiegableSettlement : Settlement
{
    [SerializeField] private List<UnitData> startingGarrisonUnits;

    protected PartyController garrison;

    public PartyController GarrisonParty => garrison;

    protected SiegeController siegeController;

    public SiegeController SiegeController => siegeController;

    [SerializeField] private List<PartyPresence> parties;
    public List<PartyPresence> Parties => parties;


    protected override void Start()
    {
        base.Start();
        InitializeGarrison();
        TickManager.Instance.OnWeekPassed += AwardGarrisonXP;
        siegeController = GetComponent<SiegeController>();
    }

    protected virtual void InitializeGarrison()
    {
        foreach (UnitData unitData in startingGarrisonUnits)
        {
            garrison.AddUnit(new UnitInstance(unitData));
        }
        garrison.SetIsHealing(true);
    }


    public abstract void AwardGarrisonXP(int i);

    /// <summary>
    /// Handles the settlement aspect of starting a siege then siegecontroller does the rest
    /// </summary>
    /// <param name="siegingParty"></param>
    /// <param name="isPlayer"></param>
    public virtual void OnSiegeStart(PartyPresence siegingParty, bool isPlayer)
    {
        Debug.Log($"{name} siege started.");

        siegeController.StartSiege(siegingParty, isPlayer);
    }


    /// <summary>
    /// Handles the settlement aspect of ending a siege then siegecontroller does the rest
    /// </summary>
    public virtual void OnSiegeEnd()
    {
        siegeController.EndSiege();
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

    public void WaitInsideCastle()
    {
        // open wait menu
        AddParty(GameManager.Instance.player.GetComponent<PartyPresence>());
        GameManager.Instance.player.GetComponent<PartyPresence>().WaitInFief();
        UIManager.Instance.OpenWaitingMenu(new WaitingMenuContext(false, this));
    }




}

