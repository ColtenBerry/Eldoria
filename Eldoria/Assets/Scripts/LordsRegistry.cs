using System.Collections.Generic;
using UnityEngine;

public class LordRegistry : MonoBehaviour
{
    public static LordRegistry Instance { get; private set; }

    [SerializeField] private List<LordProfileSO> lordSources = new();

    private Dictionary<LordProfileSO, LordProfile> lordLookup = new();
    private Dictionary<Faction, List<LordProfile>> factionLords = new();
    private List<LordProfile> allLords = new();

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    public void Initialize()
    {
        foreach (var source in lordSources)
        {
            var profile = new LordProfile(source);
            lordLookup[source] = profile;
            allLords.Add(profile);

            var faction = profile.Faction;
            if (!factionLords.ContainsKey(faction))
                factionLords[faction] = new List<LordProfile>();
            factionLords[faction].Add(profile);
        }
    }

    public void Register(LordProfile profile)
    {
        if (profile == null || profile.SourceData == null)
        {
            Debug.LogWarning("Attempted to register a null or uninitialized LordProfile.");
            return;
        }

        // Add to master list
        if (!allLords.Contains(profile))
            allLords.Add(profile);

        // Map SO to runtime profile
        lordLookup[profile.SourceData] = profile;

        // Map faction to lord
        var faction = profile.Faction;
        if (faction != null)
        {
            if (!factionLords.ContainsKey(faction))
                factionLords[faction] = new List<LordProfile>();

            if (!factionLords[faction].Contains(profile))
                factionLords[faction].Add(profile);
        }
    }

    public List<LordProfile> GetAllLords() => new(allLords);
    public List<LordProfile> GetLordsOfFaction(Faction faction) =>
        factionLords.TryGetValue(faction, out var list) ? list : new();

    public LordProfile GetLordBySO(LordProfileSO source) =>
        lordLookup.TryGetValue(source, out var profile) ? profile : null;
}

