using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LordProfile
{
    private CharacterInstance lord;
    private PartyPresence activeParty;
    private Faction faction;
    private List<UnitData> startingUnits;

    public CharacterInstance Lord => lord;
    public PartyPresence ActiveParty => activeParty;
    public Faction Faction => faction;
    public List<UnitData> StartingUnits => startingUnits;

    public LordProfileSO SourceData { get; private set; }


    public LordProfile(LordProfileSO data)
    {
        lord = new CharacterInstance(data.lordData);

        activeParty = null; // Can be assigned later when the party is spawned

        faction = data.faction;

        startingUnits = data.startingUnits;
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
}
