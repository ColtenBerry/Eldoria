using System;
using System.Collections.Generic;
using UnityEngine;

public class PartyController : MonoBehaviour
{

    [Header("Party Settings")]
    // public List<UnitData> startingUnits;
    [SerializeField]
    private List<SoldierInstance> partyMembers = new();

    [HideInInspector] int maxPartyMembers = 20;

    public List<SoldierInstance> PartyMembers => partyMembers;
    public int MaxPartyMembers => maxPartyMembers;

    public delegate void PartyChanged(); //This fcrces any listener to only activate void events? 
    public event PartyChanged OnPartyUpdated;


    [Header("Prisoner Section")]
    public List<UnitInstance> Prisoners { get; private set; } = new();
    public event Action OnPrisonersUpdated;

    [Header("Party Identity")]
    [SerializeField] private string partyID;
    public string PartyID => partyID;
    private bool isHealing = false;


    private void Awake()
    {
        if (string.IsNullOrEmpty(partyID))
            partyID = Guid.NewGuid().ToString();

        InitializeParty();
    }

    private void InitializeParty()
    {
        //PartyMembers.Clear(); //probably not necessary

    }

    public bool AddUnit(SoldierInstance unit)
    {
        if (partyMembers.Count == maxPartyMembers)
        {
            Debug.Log("Attempting to add more units than max allowed");
            return false;
        }
        Debug.Log("adding unit: controller");
        PartyMembers.Add(unit);
        OnPartyUpdated?.Invoke();
        return true;
    }

    public void RemoveUnit(SoldierInstance member)
    {
        PartyMembers.Remove(member);
        OnPartyUpdated?.Invoke();
    }

    public void SetMaxPartyMembers(int num)
    {
        maxPartyMembers = num;
    }

    public void AddPrisoner(UnitInstance prisoner)
    {
        Prisoners.Add(prisoner);
        OnPrisonersUpdated?.Invoke();
    }

    public void ReleasePrisoner(UnitInstance prisoner)
    {
        if (Prisoners.Remove(prisoner))
            OnPrisonersUpdated?.Invoke();
    }

    public void ClearPartyMembers()
    {
        partyMembers.Clear();
        OnPartyUpdated?.Invoke();
    }

    public void ClearPrisoners()
    {
        Prisoners.Clear();
        OnPrisonersUpdated?.Invoke();
    }

    public int CalculateWeeklyUpkeep()
    {
        int totalUpkeep = 0;
        foreach (SoldierInstance unit in PartyMembers)
        {
            totalUpkeep += unit.soldierData.upkeepCostPerWeek;
        }
        return totalUpkeep;
    }

    /// <summary>
    /// I dislike this, it is so that PartyUI can refresh
    /// </summary>
    public void NotifyPartyUpdated()
    {
        OnPartyUpdated?.Invoke();
    }

    public int CalculateFoodConsumption()
    {
        int totalConsumptionPerDay = 0;
        foreach (SoldierInstance unit in PartyMembers)
        {
            totalConsumptionPerDay += unit.soldierData.foodConsumptionPerDay;
        }
        return totalConsumptionPerDay;
    }
    private bool isStarving = false;
    public void SetIsStarving(bool b)
    {
        isStarving = b;
    }
    public void HandleStarvation(int foodShortage)
    {
        // for now lets just redudce health by 1 for each unit. later we can include foodshortage
        foreach (SoldierInstance unit in PartyMembers)
        {
            unit.TakeDamage(1);
        }
    }

    public void HealUnitsOverTime(int healRate)
    {
        foreach (var unit in partyMembers)
        {
            unit.Heal(healRate);
        }
    }

    public void SetIsHealing(bool b)
    {
        if (isStarving) return;
        isHealing = b;

        if (b)
        {
            TickManager.Instance.OnDayPassed += HandleTick;
        }
        else
        {
            TickManager.Instance.OnDayPassed -= HandleTick;
        }
    }

    int HEALRATE = 1;

    private void HandleTick(int i)
    {
        HealUnitsOverTime(HEALRATE);
    }

    private void OnDestroy()
    {
        // free prisoners
        foreach (UnitInstance unit in Prisoners)
        {
            if (unit is CharacterInstance character)
            {
                // free character
                LordProfile p = LordRegistry.Instance.GetLordByName(character.UnitName);
                FactionWarManager warManager = FactionsManager.Instance.GetWarManager(p.Faction);
                if (warManager == null)
                {
                    // faction must have been a bandit
                    Debug.Log("bandit was freed");
                }
                else
                {
                    warManager.AddToPendingRespawns(p);
                }
            }
        }
        // free the rest
        Prisoners.Clear();
    }


}
