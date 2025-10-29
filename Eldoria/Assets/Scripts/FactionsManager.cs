using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FactionsManager : MonoBehaviour
{
    public static FactionsManager Instance { get; private set; }

    public List<Faction> AllFactions { get; private set; }

    [SerializeField] private List<FactionWarPair> initialWars = new();

    [SerializeField]
    private List<FactionPartyList> factionPartyView = new();
    private Dictionary<Faction, List<PartyPresence>> factionParties = new();
    private Dictionary<Faction, HashSet<Settlement>> factionSettlements = new();


    private Dictionary<Faction, HashSet<Faction>> allies = new();
    private Dictionary<Faction, HashSet<Faction>> enemies = new();
    public bool AreAllied(Faction a, Faction b) => allies[a].Contains(b);
    public bool AreEnemies(Faction a, Faction b) => enemies[a].Contains(b);


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (AllFactions == null)
            AllFactions = new List<Faction>();
    }

    private void Start()
    {
        InitializeWars();
    }
    public Faction GetFactionByName(string name) =>
        AllFactions.FirstOrDefault(f => f.factionName == name);

    public void DeclareWar(Faction a, Faction b)
    {
        allies[a].Remove(b);
        allies[b].Remove(a);
        enemies[a].Add(b);
        enemies[b].Add(a);
    }

    public void FormAlliance(Faction a, Faction b)
    {
        enemies[a].Remove(b);
        enemies[b].Remove(a);
        allies[a].Add(b);
        allies[b].Add(a);
    }

    private void InitializeWars()
    {
        foreach (var pair in initialWars)
        {
            if (pair.factionA == null || pair.factionB == null) continue;

            EnsureRelationKeys(pair.factionA, pair.factionB);
            DeclareWar(pair.factionA, pair.factionB);
        }
    }

    private void EnsureRelationKeys(Faction a, Faction b)
    {
        if (!allies.ContainsKey(a)) allies[a] = new HashSet<Faction>();
        if (!allies.ContainsKey(b)) allies[b] = new HashSet<Faction>();
        if (!enemies.ContainsKey(a)) enemies[a] = new HashSet<Faction>();
        if (!enemies.ContainsKey(b)) enemies[b] = new HashSet<Faction>();
    }


    public void RegisterParty(PartyPresence party)
    {

        var faction = party.Lord.Faction;

        // Add to AllFactions if missing
        if (!AllFactions.Contains(faction))
            AllFactions.Add(faction);

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

[System.Serializable]
public class FactionWarPair
{
    public Faction factionA;
    public Faction factionB;
}

