using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-90)]
public class TerritoryManager : MonoBehaviour
{
    public static TerritoryManager Instance { get; private set; }

    [Header("Initial Territory Assignments")]
    [SerializeField] private List<TerritoryAssignment> initialAssignments = new();

    private Dictionary<Settlement, LordProfile> settlementToLord = new();
    private Dictionary<LordProfile, List<Settlement>> lordToSettlements = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }


    // Called by Game Manager
    public void InitializeOwnership(List<LordProfile> allLords)
    {
        foreach (var assignment in initialAssignments)
        {
            if (assignment.settlement == null || assignment.ownerData == null) continue;

            LordProfile matchingLord = allLords.Find(l => l.SourceData == assignment.ownerData);
            if (matchingLord != null)
            {
                RegisterOwnership(assignment.settlement, matchingLord);
            }
            else
            {
                Debug.LogWarning($"No LordProfile found for {assignment.ownerData.name}");
            }
        }
    }

    public void RegisterOwnership(Settlement settlement, LordProfile lord)
    {
        if (settlementToLord.TryGetValue(settlement, out var oldLord))
        {
            if (oldLord != null && lordToSettlements.ContainsKey(oldLord))
                lordToSettlements[oldLord].Remove(settlement);
        }

        settlementToLord[settlement] = lord;
        Debug.Log($"{lord.Lord.UnitName} is the new lord of {settlement.name}");

        if (!lordToSettlements.ContainsKey(lord))
            lordToSettlements[lord] = new List<Settlement>();

        if (!lordToSettlements[lord].Contains(settlement))
        {
            lordToSettlements[lord].Add(settlement);
            settlement.ApplyVisuals();
        }

        if (settlement is IHasBoundVillages binder)
        {
            foreach (var village in binder.BoundVillages)
            {
                RegisterOwnership(village, lord);
            }
        }
    }

    public LordProfile GetLordOf(Settlement settlement) =>
        settlementToLord.TryGetValue(settlement, out var lord) ? lord : null;

    public List<Settlement> GetSettlementsOf(LordProfile lord)
    {
        Debug.Log("Attempting to get settlements of lord by name: " + lord.Lord.UnitName);
        return lordToSettlements.TryGetValue(lord, out var list) ? list : new List<Settlement>();
    }

    public List<Settlement> GetSettlementsOfFaction(Faction faction)
    {
        List<Settlement> result = new();
        foreach (var kvp in settlementToLord)
        {
            if (kvp.Value.Faction == faction)
                result.Add(kvp.Key);
        }
        return result;
    }

    public List<T> GetSettlementsOfType<T>(LordProfile lord) where T : Settlement
    {
        return GetSettlementsOf(lord).FindAll(s => s is T).ConvertAll(s => s as T);
    }
}


[System.Serializable]
public class TerritoryAssignment
{
    public Settlement settlement;
    public LordProfileSO ownerData;
}
