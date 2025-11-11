using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Strategic AI for a faction. Coordinates lord orders based on territory threats and opportunities.
/// </summary>
public class FactionWarManager : MonoBehaviour
{
    public Faction owningFaction;

    private Dictionary<LordProfile, FactionOrder> issuedOrders = new();
    private List<LordProfile> pendingRespawns = new();

    private int tickCounter = 0;
    private const int strategicTickInterval = 20;

    private void Awake()
    {
        TickManager.Instance.OnTick += OnTick;
    }

    private void OnDestroy()
    {
        if (TickManager.Instance != null)
            TickManager.Instance.OnTick -= OnTick;
    }

    private void Start()
    {
    }

    private void OnTick(int tick)
    {
        tickCounter++;
        if (tickCounter >= strategicTickInterval)
        {
            tickCounter = 0;
            IssueAttackOrders();
        }
    }

    /// <summary>
    /// Called when a lord's party is destroyed. Adds them to the respawn queue.
    /// </summary>
    public void NotifyPartyDestroyed(LordProfile lord)
    {
        if (!pendingRespawns.Contains(lord))
        {
            pendingRespawns.Add(lord);
            Debug.Log($"☠️ Lord {lord.Lord.UnitName}'s party destroyed. Marked for respawn.");
        }
    }

    /// <summary>
    /// Called by a settlement when it is besieged. Triggers immediate defensive orders.
    /// </summary>
    public void NotifySettlementUnderSiege(Settlement settlement)
    {
        if (settlement.GetFaction() != owningFaction) return;

        Debug.Log($"⚠️ {settlement.name} is under siege! Notifying nearby lords to defend.");

        LordProfile owner = TerritoryManager.Instance.GetLordOf(settlement);
        Debug.Log($"Owner of {settlement.name} is {owner.Lord.UnitName}");
        if (owner != null && owner.ActiveParty != null)
        {
            AssignDefendOrder(owner, settlement);
        }

        List<LordProfile> nearbyLords = LordRegistry.Instance
            .GetLordsOfFaction(owningFaction)
            .Where(l => l != owner && l.ActiveParty != null)
            .Where(l => Vector3.Distance(l.ActiveParty.transform.position, settlement.transform.position) < 100f)
            .ToList();

        foreach (var lord in nearbyLords)
        {
            AssignDefendOrder(lord, settlement);
        }
    }

    private void AssignDefendOrder(LordProfile lord, Settlement settlement)
    {
        FactionOrder defendOrder = new FactionOrder(FactionOrderType.Defend, settlement.transform.position, settlement);
        issuedOrders[lord] = defendOrder;

        LordNPCStateMachine stateMachine = lord.ActiveParty.GetComponent<LordNPCStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.currentOrder = defendOrder;
            Debug.Log($"🛡️ Assigned defend order to {lord.Lord.UnitName} for {settlement.name}");
        }
    }

    private void IssueAttackOrders()
    {
        List<LordProfile> availableLords = FactionsManager.Instance.GetLordsOfFaction(owningFaction)
            .Where(l => l.ActiveParty != null && !issuedOrders.ContainsKey(l))
            .ToList();

        if (availableLords.Count == 0) return;

        List<Settlement> enemyCastles = FactionsManager.Instance.GetEnemiesOf(owningFaction)
            .SelectMany(f => TerritoryManager.Instance.GetSettlementsOfFaction(f))
            .Where(s => s is Castle)
            .ToList();

        foreach (var lord in availableLords)
        {
            Vector3 lordPos = lord.ActiveParty.transform.position;

            List<Settlement> closestTargets = enemyCastles
                .OrderBy(s => Vector3.Distance(s.transform.position, lordPos))
                .Take(5)
                .ToList();

            if (closestTargets.Count == 0) continue;

            Settlement target = closestTargets[Random.Range(0, closestTargets.Count)];
            Debug.Log($"Target Castle name is {target.name}");
            Debug.Log($"Chosen lord name is {lord.Lord.UnitName}");
            FactionOrder attackOrder = new FactionOrder(FactionOrderType.Attack, target.gameObject.transform.position, target);
            issuedOrders[lord] = attackOrder;

            LordNPCStateMachine stateMachine = lord.ActiveParty.GetComponent<LordNPCStateMachine>();
            if (stateMachine != null)
            {
                stateMachine.currentOrder = attackOrder;
                Debug.Log($"⚔️ Assigned attack order to {lord.Lord.UnitName} targeting {target.name}");
            }
        }
    }

    public void ClearOrder(LordProfile lord)
    {
        if (issuedOrders.Remove(lord))
        {
            Debug.Log($"🧹 Cleared order for {lord.Lord.UnitName}");
        }
    }

}
