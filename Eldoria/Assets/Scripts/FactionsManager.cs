using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[DefaultExecutionOrder(-80)]
public class FactionsManager : MonoBehaviour
{
    public static FactionsManager Instance { get; private set; }

    public List<Faction> AllFactions { get; private set; }

    [SerializeField] private List<FactionWarPair> initialWars = new();

    [SerializeField]
    private List<FactionPartyList> factionPartyView = new();
    private Dictionary<Faction, List<PartyPresence>> factionParties = new();


    private Dictionary<Faction, List<Faction>> allies = new();
    private Dictionary<Faction, List<Faction>> enemies = new();



    private Dictionary<Faction, FactionWarManager> warManagers = new();

    public bool AreAllied(Faction a, Faction b)
    {
        return allies[a].Contains(b) || a == b;
    }

    public bool AreEnemies(Faction a, Faction b)
    {
        return enemies.TryGetValue(a, out var enemySet) && enemySet.Contains(b);
    }
    public List<Faction> GetEnemiesOf(Faction faction)
    {
        return enemies[faction];
    }


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
        InitializeFactionsFromLords();
        InitializeWars();
        CreateWarManagers();
    }

    private void CreateWarManagers()
    {
        foreach (var faction in AllFactions)
        {
            GameObject managerGO = new GameObject($"WarManager_{faction.factionName}");
            managerGO.transform.SetParent(transform); // optional: keep hierarchy clean

            FactionWarManager manager = managerGO.AddComponent<FactionWarManager>();
            manager.owningFaction = faction;

            warManagers[faction] = manager;
        }
    }

    public FactionWarManager GetWarManager(Faction faction)
    {
        warManagers.TryGetValue(faction, out var manager);
        return manager;
    }


    public Faction GetFactionByName(string name) =>
        AllFactions.FirstOrDefault(f => f.factionName == name);

    public void DeclareWar(Faction a, Faction b)
    {
        allies[a].Remove(b);
        allies[b].Remove(a);
        enemies[a].Add(b);
        enemies[b].Add(a);

        UIManager.Instance.LogMessage(new WorldMessage($"{a.name} declares war on {b.name}"));
    }

    public void DeclarePeace(Faction a, Faction b)
    {
        // Ensure both factions exist in the dictionaries
        EnsureRelationKeys(a, b);

        // Remove from allies if present
        allies[a].Remove(b);
        allies[b].Remove(a);

        // Remove from enemies if present
        enemies[a].Remove(b);
        enemies[b].Remove(a);

        UIManager.Instance.LogMessage(new WorldMessage($"{a.name} makes peace with {b.name}"));

    }


    public void FormAlliance(Faction a, Faction b)
    {
        enemies[a].Remove(b);
        enemies[b].Remove(a);
        allies[a].Add(b);
        allies[b].Add(a);
    }

    public void InitializeFactionsFromLords()
    {
        AllFactions = LordRegistry.Instance
            .GetAllLords()
            .Select(l => l.Faction)
            .Where(f => f != null)
            .Distinct()
            .ToList();

        foreach (var faction in AllFactions)
        {
            if (!allies.ContainsKey(faction)) allies[faction] = new List<Faction>();
            if (!enemies.ContainsKey(faction)) enemies[faction] = new List<Faction>();
        }
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
        if (!allies.ContainsKey(a)) allies[a] = new List<Faction>();
        if (!allies.ContainsKey(b)) allies[b] = new List<Faction>();
        if (!enemies.ContainsKey(a)) enemies[a] = new List<Faction>();
        if (!enemies.ContainsKey(b)) enemies[b] = new List<Faction>();
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

    public int GetFactionStrength(Faction faction)
    {
        if (!factionParties.ContainsKey(faction)) return 0;

        return factionParties[faction]
            .Where(p => p != null)
            .Sum(p => p.GetStrengthEstimate());
    }

    public List<Settlement> GetSettlementsOfFaction(Faction faction)
    {
        return TerritoryManager.Instance.GetSettlementsOfFaction(faction);
    }

    public List<LordProfile> GetLordsOfFaction(Faction faction)
    {
        return LordRegistry.Instance.GetLordsOfFaction(faction);
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

