using System;
using System.Collections.Generic;
using UnityEngine;

public class PartyController : MonoBehaviour
{

    [Header("Party Settings")]
    // public List<UnitData> startingUnits;
    [SerializeField]
    private List<UnitInstance> partyMembers = new();

    [HideInInspector] int maxPartyMembers = 20;

    public List<UnitInstance> PartyMembers => partyMembers;
    public int MaxPartyMembers => maxPartyMembers;

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

    public bool AddUnit(UnitInstance unit)
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

    public void RemoveUnit(UnitInstance member)
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

    public void ClearPrisoners()
    {
        Prisoners.Clear();
    }

}
