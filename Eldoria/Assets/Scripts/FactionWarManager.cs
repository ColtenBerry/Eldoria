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

    [SerializeField] private int lastCastleAttackTick = -100; // ensures first attack is allowed
    [SerializeField] private const int castleAttackCooldown = 30;

    private bool CanLaunchCastleAttack(int currentTick)
    {
        return currentTick - lastCastleAttackTick >= castleAttackCooldown;
    }

    private void Awake()
    {
        TickManager.Instance.OnTick += OnTick;
    }

    private void OnDestroy()
    {
        if (TickManager.Instance != null)
            TickManager.Instance.OnTick -= OnTick;
    }

    private void OnTick(int tick)
    {
        tickCounter++;
        if (tickCounter >= strategicTickInterval)
        {
            tickCounter = 0;
            IssueStrategicOrders();
        }
    }

    public void NotifyPartyDestroyed(LordProfile lord)
    {
        if (!pendingRespawns.Contains(lord))
        {
            pendingRespawns.Add(lord);
            ClearOrder(lord);
            Debug.Log($"☠️ Lord {lord.Lord.UnitName}'s party destroyed. Marked for respawn.");
        }
    }

    public void NotifySettlementUnderSiege(Settlement settlement)
    {
        if (settlement.GetFaction() != owningFaction) return;

        Debug.Log($"⚠️ {settlement.name} is under siege! Prioritizing defense.");

        LordProfile owner = TerritoryManager.Instance.GetLordOf(settlement);
        if (owner != null && owner.ActiveParty != null)
        {
            AssignDefendOrder(owner, settlement);
        }

        List<LordProfile> nearbyLords = LordRegistry.Instance
            .GetLordsOfFaction(owningFaction)
            .Where(l => l != owner && l.ActiveParty != null)
            .Where(l => Vector3.Distance(l.ActiveParty.transform.position, settlement.transform.position) < 150f)
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

    /// <summary>
    /// Cancels all defense orders for a specific castle (e.g., when it is conquered).
    /// Called directly by the castle's SiegeController or Settlement logic.
    /// </summary>
    public void CancelDefense(Settlement settlement)
    {
        Debug.Log($"❌ CancelDefense called for {settlement.name}");

        // Find all lords currently defending this settlement
        var lordsWithDefense = issuedOrders
            .Where(kvp => kvp.Value != null &&
                          kvp.Value.Type == FactionOrderType.Defend &&
                          ((Object)kvp.Value.TargetObject).GetInstanceID() == settlement.GetInstanceID())
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var lord in lordsWithDefense)
        {
            issuedOrders.Remove(lord);

            if (lord.ActiveParty == null)
            {
                Debug.LogWarning($"{lord.Lord.UnitName} has no active party (destroyed). Skipping cleanup.");
                continue;
            }

            var stateMachine = lord.ActiveParty.GetComponent<LordNPCStateMachine>();
            if (stateMachine != null)
            {
                stateMachine.currentOrder = null;
            }

            Debug.Log($"✅ {lord.Lord.UnitName} defense order cancelled for {settlement.name}");
        }
    }


    private void IssueStrategicOrders()
    {
        List<LordProfile> availableLords = FactionsManager.Instance.GetLordsOfFaction(owningFaction)
            .Where(l => l.ActiveParty != null && !issuedOrders.ContainsKey(l))
            .ToList();

        if (availableLords.Count == 0) return;

        // --- Step 1: Prioritize defense ---
        List<Settlement> threatenedSettlements = TerritoryManager.Instance.GetSettlementsOfFaction(owningFaction)
            .Where(s => s.TryGetComponent<SiegeController>(out var sc) && sc.IsUnderSiege)
            .ToList();

        if (threatenedSettlements.Count > 0)
        {
            Debug.Log($"⚠️ Defense priority: {threatenedSettlements.Count} settlements under siege.");
            foreach (var settlement in threatenedSettlements)
            {
                List<LordProfile> defenders = availableLords
                    .OrderBy(l => Vector3.Distance(l.ActiveParty.transform.position, settlement.transform.position))
                    .Take(3)
                    .ToList();

                foreach (var lord in defenders)
                {
                    AssignDefendOrder(lord, settlement);
                    availableLords.Remove(lord);
                }
            }
        }

        if (availableLords.Count == 0) return;

        // --- Step 2: Consider attacks ---
        if (CanLaunchCastleAttack(TickManager.Instance.TickCount))
        {
            // set tick
            lastCastleAttackTick = TickManager.Instance.TickCount;
            Debug.Log("planning attack");

            List<SiegeController> enemySieges = FactionsManager.Instance.GetEnemiesOf(owningFaction)
        .SelectMany(f => TerritoryManager.Instance.GetSettlementsOfFaction(f))
        .OfType<Castle>() // only castles
        .Select(c => c.GetComponent<SiegeController>()) // grab the SiegeController
        .Where(sc => sc != null) // filter out castles without one
        .ToList();


            foreach (SiegeController target in enemySieges)
            {
                if (availableLords.Count == 0) break;

                int defenderStrength = target.GetTotalDefenderStrength();
                Debug.Log($"🔍 Evaluating {target.name}: defenders={defenderStrength}");

                List<LordProfile> nearbyLords = availableLords
                    .Where(l => Vector3.Distance(l.ActiveParty.transform.position, target.transform.position) < 300f)
                    .OrderBy(l => Vector3.Distance(l.ActiveParty.transform.position, target.transform.position))
                    .ToList();

                int friendlyStrength = 0;
                List<LordProfile> assignedForce = new();

                foreach (var lord in nearbyLords)
                {
                    if (lord.ActiveParty.PartyController.PartyMembers.Count < lord.ActiveParty.PartyController.MaxPartyMembers * 0.5f)
                    {
                        Debug.Log($"{lord.Lord.UnitName} is too weakened to join the siege.");
                        continue;
                    }
                    int strength = lord.ActiveParty.GetStrengthEstimate();
                    friendlyStrength += strength;
                    assignedForce.Add(lord);
                    Debug.Log($"➕ Adding {lord.Lord.UnitName} (strength={strength}), total={friendlyStrength}");

                    if (friendlyStrength >= defenderStrength * 2.5f) break; // 2.5 : 1 ratio
                }

                float ratio = defenderStrength > 0 ? (float)friendlyStrength / defenderStrength : 999f;
                Debug.Log($"📊 Final ratio vs {target.name}: {ratio:F2} ({friendlyStrength} vs {defenderStrength})");

                if (ratio >= 2.0f && assignedForce.Count >= 3)
                {
                    foreach (var lord in assignedForce)
                    {
                        AssignAttackOrder(lord, target.Settlement);
                        availableLords.Remove(lord);
                    }
                    Debug.Log($"⚔️ Attack launched on {target.name} with {assignedForce.Count} lords.");
                }
                else
                {
                    Debug.Log($"❌ Attack aborted on {target.name}: insufficient force.");
                }
            }
        }
        // --- Step 3: Fallback tasks ---
        foreach (var lord in availableLords)
        {
            AssignFallbackOrder(lord);
        }
    }

    private void AssignAttackOrder(LordProfile lord, Settlement target)
    {
        FactionOrder attackOrder = new FactionOrder(FactionOrderType.Attack, target.transform.position, target);
        issuedOrders[lord] = attackOrder;

        LordNPCStateMachine stateMachine = lord.ActiveParty.GetComponent<LordNPCStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.currentOrder = attackOrder;
            Debug.Log($"⚔️ Assigned attack order to {lord.Lord.UnitName} targeting {target.name}");
        }
    }

    private void AssignFallbackOrder(LordProfile lord)
    {
        int roll = Random.Range(0, 3);
        Settlement home = TerritoryManager.Instance.GetSettlementsOf(lord).FirstOrDefault();

        if (roll == 0 && home != null)
        {
            AssignDefendOrder(lord, home);
        }
        else if (roll == 1 && home != null)
        {
            FactionOrder patrolOrder = new FactionOrder(FactionOrderType.Defend, home.transform.position, home);
            issuedOrders[lord] = patrolOrder;
            var stateMachine = lord.ActiveParty.GetComponent<LordNPCStateMachine>();
            if (stateMachine != null) stateMachine.currentOrder = patrolOrder;
            Debug.Log($"🚶 Assigned patrol order to {lord.Lord.UnitName} around {home.name}");
        }
        else
        {
            issuedOrders[lord] = null; // idle / develop lands
            Debug.Log($"🌾 {lord.Lord.UnitName} is idle, developing lands.");
        }
    }

    public void ClearOrder(LordProfile lord)
    {
        if (issuedOrders.Remove(lord))
        {
            Debug.Log($"🧹 Cleared order for {lord.Lord.UnitName}");
        }
    }

    /// <summary>
    /// Wipes an order for all lords who share it (e.g., when a castle is conquered).
    /// </summary>
    public void CompleteOrder(FactionOrder order)
    {
        Debug.Log("completeOrder called!");

        var lordsWithOrder = issuedOrders
            .Where(kvp => kvp.Value != null &&
                          kvp.Value.Type == order.Type &&
                          kvp.Value.TargetObject == order.TargetObject)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var lord in lordsWithOrder)
        {
            Debug.Log($"{lord.Lord.UnitName} has a completed order. attempting to remove order");
            issuedOrders.Remove(lord);

            var stateMachine = lord.ActiveParty.GetComponent<LordNPCStateMachine>();
            if (stateMachine != null) stateMachine.currentOrder = null;

            Debug.Log($"✅ {lord.Lord.UnitName} completed {order.Type} on {order.TargetObject}");
        }
    }
}
