using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LordProfile
{
    private CharacterInstance lord;
    private List<Settlement> ownedTerritories;
    private PartyPresence activeParty;
    private Faction faction;
    private List<UnitData> startingUnits;

    public CharacterInstance Lord => lord;
    public List<Settlement> OwnedTerritories => ownedTerritories;
    public PartyPresence ActiveParty => activeParty;
    public Faction Faction => faction;
    public List<UnitData> StartingUnits => startingUnits;


    public LordProfile(LordProfileSO data)
    {
        lord = new CharacterInstance(data.lordData);
        ownedTerritories = new List<Settlement>();

        foreach (Settlement territoryData in data.ownedTerritories)
        {
            ownedTerritories.Add(territoryData);
        }
        activeParty = null; // Can be assigned later when the party is spawned

        faction = data.faction;

        startingUnits = data.startingUnits;
    }

    public void AddLand(Settlement settlement)
    {
        if (!OwnedTerritories.Contains(settlement)) OwnedTerritories.Add(settlement);
    }

    public void RemoveLand(Settlement settlement)
    {
        OwnedTerritories.Remove(settlement);
    }

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
}
