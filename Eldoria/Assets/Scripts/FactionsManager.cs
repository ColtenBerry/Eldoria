using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FactionsManager : MonoBehaviour
{
    public static FactionsManager Instance { get; private set; }

    [SerializeField]
    private List<FactionPartyList> factionPartyView = new();
    private Dictionary<Faction, List<PartyPresence>> factionParties = new();
    private Dictionary<Faction, HashSet<Settlement>> factionSettlements = new();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterParty(PartyPresence party)
    {
        var faction = party.Lord.Faction;

        // Update internal dictionary
        if (!factionParties.ContainsKey(faction))
            factionParties[faction] = new List<PartyPresence>();
        factionParties[faction].Add(party);

        // Update inspector view
        var viewEntry = factionPartyView.FirstOrDefault(f => f.faction == faction);
        if (viewEntry == null)
        {
            viewEntry = new FactionPartyList { faction = faction, parties = new List<PartyPresence>() };
            factionPartyView.Add(viewEntry);
        }
        viewEntry.parties.Add(party);
    }

    public void UnregisterParty(PartyPresence party)
    {
        var faction = party.Lord.Faction;

        // Update internal dictionary
        if (factionParties.ContainsKey(faction))
            factionParties[faction].Remove(party);

        // Update inspector view
        var viewEntry = factionPartyView.FirstOrDefault(f => f.faction == faction);
        viewEntry?.parties.Remove(party);
    }

    public void RegisterTerritory(Settlement territory, Faction faction)
    {
        if (!factionSettlements.ContainsKey(faction))
            factionSettlements[faction] = new HashSet<Settlement>();

        factionSettlements[faction].Add(territory);
    }

    public void UnregisterTerritory(Settlement territory, Faction faction)
    {
        if (factionSettlements.ContainsKey(faction))
            factionSettlements[faction].Remove(territory);
    }

    public int GetFactionStrength(Faction faction)
    {
        if (!factionParties.ContainsKey(faction)) return 0;

        return factionParties[faction]
            .Where(p => p != null)
            .Sum(p => p.GetStrengthEstimate());
    }
}

[System.Serializable]
public class FactionPartyList
{
    public Faction faction;
    public List<PartyPresence> parties;
}

