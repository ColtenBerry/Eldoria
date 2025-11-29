using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LordProfile
{
    private CharacterInstance lord;
    private PartyPresence activeParty;
    private Faction faction;
    private List<SoldierData> startingUnits;
    private int goldAmount;

    public CharacterInstance Lord => lord;
    public PartyPresence ActiveParty => activeParty;
    public Faction Faction => faction;
    public int GoldAmount => goldAmount;
    public List<SoldierData> StartingUnits => startingUnits;

    public LordProfileSO SourceData { get; private set; }


    public LordProfile(LordProfileSO data)
    {
        lord = new CharacterInstance(data.lordData);

        activeParty = null; // Can be assigned later when the party is spawned

        faction = data.faction;

        startingUnits = data.startingSoldiers.soldiers;
        goldAmount = data.startingGold;
        SourceData = data;
    }

    public List<Settlement> GetOwnedTerritories() =>
    TerritoryManager.Instance.GetSettlementsOf(this);

    public List<T> GetOwnedSettlementsOfType<T>() where T : Settlement =>
        TerritoryManager.Instance.GetSettlementsOfType<T>(this);

    public void RemoveActiveParty()
    {
        activeParty = null;
    }

    public void AddActiveParty(PartyPresence party)
    {
        if (activeParty != null)
        {
            UnityEngine.Debug.LogWarning($"Lord {lord.UnitName} already has an active party.");
            return;
        }

        activeParty = party;
        party.SetLordProfile(this);
    }


    // Gold System

    public bool CanAfford(int amount) => goldAmount >= amount;

    public bool TrySpendGold(int amount)
    {
        if (!CanAfford(amount)) return false;
        goldAmount -= amount;
        return true;
    }

    public void AddGold(int amount)
    {
        goldAmount += amount;
    }

    public void ChangeGold(int amount)
    {
        goldAmount = goldAmount + amount;
    }


    public string GetGoldSummary() => $"{lord.UnitName} has {goldAmount} gold.";
}
