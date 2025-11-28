using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SiegeController))]
public abstract class SiegeableSettlement : Settlement
{
    [SerializeField] private List<UnitData> startingGarrisonUnits;

    protected PartyController garrison;

    public PartyController GarrisonParty => garrison;

    [SerializeField] protected SiegeController siegeController;

    public PartyController SiegeController => SiegeController;

    protected override void Start()
    {
        base.Start();
        InitializeGarrison();
        TickManager.Instance.OnWeekPassed += AwardGarrisonXP;
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


}

