using System;
using System.Collections.Generic;
using UnityEngine;

public class PartyController : MonoBehaviour
{
    [Header("Lord Settings")]
    public UnitData lordData;
    public PartyMember Lord { get; private set; }

    [Header("Party Settings")]
    public List<UnitData> startingUnits;
    public List<PartyMember> PartyMembers { get; private set; } = new();

    public delegate void PartyChanged(); //This fcrces any listener to only activate void events? 
    public event PartyChanged OnPartyUpdated;


    [Header("Prisoner Section")]
    public List<PartyMember> Prisoners { get; private set; } = new();
    public event Action OnPrisonersUpdated;

    private void Awake()
    {
        InitializeParty();
    }

    private void InitializeParty()
    {
        Lord = new PartyMember(lordData);

        PartyMembers.Clear(); //probably not necessary
        foreach (var unit in startingUnits)
        {
            PartyMembers.Add(new PartyMember(unit));
            //test
            Prisoners.Add(new PartyMember(unit));
        }
        OnPartyUpdated?.Invoke();
    }

    public void AddUnit(UnitData unit)
    {
        PartyMembers.Add(new PartyMember(unit));
        OnPartyUpdated?.Invoke();
    }

    public void RemoveUnit(PartyMember member)
    {
        PartyMembers.Remove(member);
        OnPartyUpdated?.Invoke();
    }
    public int GetTotalPower()
    {
        int total = Lord.currentPower;
        foreach (PartyMember member in PartyMembers)
        {
            total += member.currentPower;
        }
        return total;
    }
    public void AddPrisoner(UnitData unitData)
    {
        var prisoner = new PartyMember(unitData);
        Prisoners.Add(prisoner);
        OnPrisonersUpdated?.Invoke();
    }

    public void ReleasePrisoner(PartyMember prisoner)
    {
        if (Prisoners.Remove(prisoner))
            OnPrisonersUpdated?.Invoke();
    }

}
