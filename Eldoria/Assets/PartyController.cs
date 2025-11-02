using System;
using System.Collections.Generic;
using UnityEngine;

public class PartyController : MonoBehaviour
{

    [Header("Party Settings")]
    // public List<UnitData> startingUnits;
    [SerializeField]
    private List<UnitInstance> partyMembers = new();

    public List<UnitInstance> PartyMembers => partyMembers;

    public delegate void PartyChanged(); //This fcrces any listener to only activate void events? 
    public event PartyChanged OnPartyUpdated;


    [Header("Prisoner Section")]
    public List<UnitInstance> Prisoners { get; private set; } = new();
    public event Action OnPrisonersUpdated;

    private void Awake()
    {
        InitializeParty();
    }

    private void InitializeParty()
    {
        //PartyMembers.Clear(); //probably not necessary
    }

    public void AddUnit(UnitInstance unit)
    {
        Debug.Log("adding unit: controller");
        PartyMembers.Add(unit);
        OnPartyUpdated?.Invoke();
    }

    public void RemoveUnit(UnitInstance member)
    {
        PartyMembers.Remove(member);
        OnPartyUpdated?.Invoke();
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

    public void ClearPrisoners()
    {
        Prisoners.Clear();
    }

}
