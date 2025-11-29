using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum NPCIntent
{
    None,
    Raid,
    SiegeCastle,
    DefendFief,
    HuntParty,
    DefendParty,
    WaitInFief,
    RecruitTroops,
    UpgradeFief,
    PatrolTerritory
}

[RequireComponent(typeof(PartyController))]
public class LordNPCStateMachine : BaseNPCStateMachine
{

    private Queue<Vector3> currentPath = new();

    public FactionOrder currentOrder;
    [SerializeField] private NPCIntent currentIntent;
    [SerializeField] private NPCIntent previousIntent;
    private LordProfile currentLord;
    private FactionWarManager factionWarManager;
    private PartyController partyController;
    private PartyPresence partyPresence;

    protected override void Awake()
    {
        base.Awake();
        partyController = GetComponent<PartyController>();
        if (partyController == null)
        {
            Debug.LogWarning("Expecte4d PartyController");
        }

        partyPresence = GetComponent<PartyPresence>();
        if (partyPresence == null)
        {
            Debug.LogWarning("Expecte4d partypresence");
        }

        StartCoroutine(WaitForLord());
    }

    private IEnumerator WaitForLord()
    {
        Debug.Log("📌 Starting WaitForLord coroutine...");

        // Wait until LordRegistry is initialized
        while (LordRegistry.Instance == null)
        {
            Debug.Log("⏳ Waiting for LordRegistry.Instance...");
            yield return null;
        }

        // Wait until the lord is found
        while (currentLord == null)
        {
            currentLord = LordRegistry.Instance.GetLordByName(gameObject.name);
            if (currentLord == null)
            {
                Debug.Log("⏳ Waiting for currentLord to be assigned...");
                yield return null;
            }
        }

        Debug.Log("✅ Current lord is " + currentLord.Lord.UnitName);

        factionWarManager = FactionsManager.Instance.GetWarManager(currentLord.Faction);
    }


    // void Initialize()
    // {

    //     //TODO: change this to a coroutine to run in awake until currentLord != null. Look at statspaneluicontroller for reference
    //     if (currentLord != null) return;
    //     currentLord = LordRegistry.Instance.GetLordByName(gameObject.name);
    //     Debug.Log("Current lord is " + currentLord.Lord.UnitName);

    // }

    /// <summary>
    /// Idle Behavior determines what behavior to call when no immediate threat or easy target is presented
    /// </summary>
    /// 
    /// 

    private void Start()
    {
        TickManager.Instance.OnWeekPassed += ResetPatrolledLands;
    }

    protected override void SetIdleBehavior()
    {
        Debug.Log($"{currentLord.Lord.UnitName} setIdleBehavior: [SetIdleBehavior] currentOrder: {(currentOrder == null ? "null" : currentOrder.Type.ToString())}, " +
          $"currentIntent: {currentIntent}, previousIntent: {previousIntent}, " +
          $"objective: {(objective == null ? "null" : objective.name)}, " +
          $"previousObjective: {(previousObjective == null ? "null" : previousObjective.name)}");
        if (currentOrder != null)
        {
            EvaluateFactionOrder();
        }
        else
        {
            EvaluateLocalIntent();
        }
        ExecuteIntentIfClose();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (currentState != NPCState.Idle && isSieging)
        {
            // end siege
            if (currentOrder == null)
            {
                Debug.LogWarning("current order is null... warning");
                return;
            }
            EndSiege(currentOrder.TargetObject as SiegableSettlement);
        }
    }


    private List<Settlement> unPatrolledLands = new();

    private GameObject previousObjective;
    [SerializeField] private GameObject objective;

    private void EvaluateFactionOrder()
    {
        Debug.Log($"[EvaluateFactionOrder] {currentLord.Lord.UnitName} Evaluating order for {currentLord.Lord.UnitName}. " +
                  $"OrderType={currentOrder.Type}, TargetObject={currentOrder.TargetObject}, " +
                  $"TargetLocation={currentOrder.TargetLocation}");
        if (currentOrder == null) return;

        currentIntent = currentOrder.Type switch
        {
            FactionOrderType.Attack => ResolveAttackIntent(currentOrder.TargetObject),
            FactionOrderType.Defend => ResolveDefendIntent(currentOrder.TargetObject),
            _ => NPCIntent.None
        };
        GameObject targetGO = (currentOrder.TargetObject as MonoBehaviour)?.gameObject;
        if (targetGO == null)
        {
            Debug.LogError("expected a monobehavior on the game object");
            return;
        }


        if (previousIntent != currentIntent || previousObjective != targetGO)
        {
            currentPath.Clear();
            GeneratePathTo(currentOrder.TargetLocation);
            Debug.Log("Generating path to: " + currentOrder.TargetLocation);

            // preserve previous intent so we can run leave-fief logic if we were waiting
            var wasWaiting = previousIntent == NPCIntent.WaitInFief;

            previousIntent = currentIntent;
            objective = targetGO;


            if (wasWaiting && partyPresence != null)
                LeaveFief(previousObjective.GetComponent<SiegableSettlement>());

            // fix: remember the objective so future checks see that we've already applied it
            previousObjective = objective;

        }
        Debug.Log("Intent: " + currentIntent);
        Debug.Log("Target location: " + currentOrder.TargetLocation);
        Debug.Log("Target object: " + (currentOrder.TargetObject as MonoBehaviour)?.name);

    }

    private void EvaluateLocalIntent()
    {

        Debug.Log($"{currentLord.Lord.UnitName} EvaluateLocalIntent: [EvaluateLocalIntent] Deciding local intent. " +
          $"Lord: {currentLord.Lord.UnitName}, ShouldDefend: {ShouldDefendFief()}, " +
          $"ShouldRecruit: {ShouldRecruit()}, ShouldPatrol: {ShouldPatrol()}");

        if (currentLord == null) return;


        if (ShouldDefendFief())
        {
            currentIntent = NPCIntent.DefendFief;
            objective = TerritoryManager.Instance.GetSettlementsOf(currentLord).First().gameObject;
        }
        else if (ShouldRecruit())
        {
            currentIntent = NPCIntent.RecruitTroops;
            var validSources = TerritoryManager.Instance.GetSettlementsOf(currentLord)
                .Where(s => s.TryGetComponent<RecruitmentSource>(out var src) && src.GetRecruitableUnits().Count > 0)
                .Select(s => s.gameObject)
                .ToList();

            objective = GetClosest(validSources);
        }
        else if (ShouldPatrol())
        {
            currentIntent = NPCIntent.PatrolTerritory;
            objective = GetClosest(unPatrolledLands.Select(s => s.gameObject).ToList());
        }
        else
        {
            currentIntent = NPCIntent.WaitInFief;
            objective = TerritoryManager.Instance.GetSettlementsOf(currentLord)
                .OrderByDescending(s => s.GetProsperity())
                .FirstOrDefault()?.gameObject;
        }

        if (objective == null) return;

        if (previousIntent != currentIntent || previousObjective != objective)
        {
            currentPath.Clear();
            GeneratePathTo(objective.transform.position);

            if (isInFief)
            {
                LeaveFief(previousObjective.GetComponent<SiegableSettlement>());
            }

            if (previousIntent == NPCIntent.SiegeCastle && previousObjective is SiegableSettlement)
            {
                if (previousObjective == null)
                {
                    Debug.LogWarning("previous objective is null...");
                    return;
                }
                EndSiege(previousObjective.GetComponent<SiegableSettlement>());
            }

            previousIntent = currentIntent;
            previousObjective = objective;
        }
    }

    private void ExecuteIntentIfClose()
    {
        Debug.Log("execute if close called");
        if (objective == null) return;

        Vector3 targetPos = objective.transform.position;

        if (IsCloseTo(targetPos))
        {
            Debug.Log("is close to : " + objective.name);
            switch (currentIntent)
            {
                case NPCIntent.RecruitTroops:
                    var source = objective.GetComponent<RecruitmentSource>();
                    Recruit(source);
                    break;
                case NPCIntent.WaitInFief:
                    SiegableSettlement sc = objective.GetComponent<SiegableSettlement>();
                    if (sc == null) Debug.LogError("siegecontroller is null");
                    WaitInFief(sc);
                    break;
                case NPCIntent.PatrolTerritory:
                    var settlement = objective.GetComponent<Settlement>();
                    PatrolLand(settlement);
                    break;
                case NPCIntent.SiegeCastle:
                    SiegableSettlement castle = objective.GetComponent<SiegableSettlement>();
                    if (castle != null)
                    {
                        Debug.Log("npc intent thing reached on castle: " + castle.name);
                        if (isSieging) break;
                        SiegeCastle(castle);
                    }
                    else
                    {
                        Debug.LogWarning("previousobjective is not a castle");
                    }

                    break;
                case NPCIntent.DefendFief:
                    DefendFief(objective.GetComponent<Settlement>());
                    break;
                default:
                    Debug.Log("something is wrong. default hit");
                    break;

            }
        }
        else
        {
            MoveTowardsNextTileInPath();
            // Debug.Log("Current path count: " + currentPath.Count);
        }
    }



    private void DefendFief(Settlement fief)
    {
        if (nearbyEnemies.Count == 0)
        {
            Debug.Log("no nearby enemies to defend against");
            ClearOrder();
            return;
        }
        fief.TryGetComponent(out SiegableSettlement sc);

        if (sc != null)
        {
            // defend castle logic
            if (!sc.SiegeController.IsUnderSiege && sc.GetFaction() == currentLord.Faction)
            {
                // join castle in defense
                EnterFief(fief as SiegableSettlement);
            }
            else
            {
                // just wait nearby i guess?
            }
        }

        else if (fief is Village)
        {
            // defend village logic
            // just sit here i guess
        }
        else
        {
            Debug.LogWarning("Expected fief to be Castle or Village");
        }

        // defend logic
    }



    /// <summary>
    /// Current Faction Order is null, assess situation and create a local decision
    /// </summary>
    private bool ShouldPatrol()
    {
        return unPatrolledLands.Count > 0;
    }

    private void PatrolLand(Settlement land)
    {
        // do patrol thing? 

        // drop the land
        unPatrolledLands.Remove(land);
    }

    private void ResetPatrolledLands(int i)
    {
        unPatrolledLands.Clear();
        unPatrolledLands.AddRange(TerritoryManager.Instance.GetSettlementsOf(currentLord));
    }

    private bool ShouldDefendFief()
    {
        return false;
        // i should get an alert saying my lands are under attack. Then i should return true
    }

    private bool ShouldRecruit()
    {
        if (partyController.MaxPartyMembers == partyController.PartyMembers.Count)
        {
            return false;
        }
        if (!currentLord.CanAfford(500)) return false; // Check if the lord can afford recruitment. at least 500 gold
        // should recruit logic
        List<Settlement> ownedSettlements = TerritoryManager.Instance.GetSettlementsOf(currentLord);
        List<Settlement> validRecruitmentSources = ownedSettlements.FindAll(settlement =>
    settlement.TryGetComponent<RecruitmentSource>(out var source) &&
    source.GetRecruitableUnits().Count > 0);
        return validRecruitmentSources.Count > 0;
    }

    private void Recruit(RecruitmentSource source)
    {
        Debug.Log("Attempting Recruit");
        if (source == null)
        {
            Debug.LogWarning("Recruitment source is null");
            return;
        }

        List<UnitInstance> recruits = new List<UnitInstance>(source.GetRecruitableUnits());

        for (int i = recruits.Count - 1; i >= 0; i--)
        {
            UnitInstance unit = recruits[i];
            Debug.Log("Attempting to recruit: " + unit.UnitName);

            if (partyController.AddUnit(unit))
            {
                RecruitmentUtility.TryRecruitUnit(unit, partyController, currentLord, source);
            }
        }
    }

    private bool isSieging = false;

    private void SiegeCastle(SiegableSettlement castle)
    {
        if (castle.GetFaction() == currentLord.Faction)
        {
            Debug.Log("Attempting to siege castle of same faction! breaking");
            CompleteOrder();
            return;
        }
        Debug.Log("lordnpcstatemachine sieging castle name: " + castle.name);
        castle.OnSiegeStart(selfPresence, false);
        isSieging = true;
    }

    public void EndSiege(SiegableSettlement castle)
    {
        isSieging = false;
        if (castle == null)
        {
            return;
        }
        castle.OnSiegeEnd();

        if (objective.TryGetComponent<SiegableSettlement>(out var sc))
        {
            if (sc.GetFaction() == currentLord.Faction)
            {
                // castle must be conquered
                CompleteOrder();
            }
        }
    }
    private bool isInFief = false;
    private void EnterFief(SiegableSettlement fief)
    {
        if (fief == null)
        {
            Debug.LogWarning("Expected fief to be a castle");
            return;
        }
        if (fief.SiegeController.IsUnderSiege)
        {
            Debug.LogWarning("Cannot enter fief that is under siege");
            return;
        }
        fief.AddParty(selfPresence);
        partyPresence.WaitInFief();
        isInFief = true;
    }
    private void LeaveFief(SiegableSettlement fief)
    {
        if (fief.SiegeController.IsUnderSiege)
        {
            Debug.LogWarning("Cannot leave fief that is under siege");
            return;
        }
        fief.RemoveParty(selfPresence);
        partyPresence.LeaveFief();
        isInFief = false;
    }
    protected override void ExecuteTransitionActions()
    {
        if (isSieging)
        {
            objective.TryGetComponent(out SiegableSettlement sc);
            EndSiege(sc);
        }

        currentPath.Clear();
        if (objective == null)
        {
            Debug.LogWarning("generatepathto will not run here!");
            return;
        }
        GeneratePathTo(objective.transform.position);
    }

    private NPCIntent ResolveAttackIntent(IInteractable target)
    {
        if (target is Castle) return NPCIntent.SiegeCastle;
        if (target is Village) return NPCIntent.Raid;
        if (target is PartyPresence) return NPCIntent.HuntParty;
        return NPCIntent.None;
    }

    private NPCIntent ResolveDefendIntent(IInteractable target)
    {
        if (target is Castle) return NPCIntent.DefendFief;
        if (target is Village) return NPCIntent.DefendFief;
        if (target is PartyPresence) return NPCIntent.DefendParty;
        return NPCIntent.None;
    }


    private void GeneratePathTo(Vector3 destination)
    {
        if (destination == null)
        {
            Debug.LogWarning("Destination is null!");
        }
        List<Vector3> path = PathfindingManager.Instance.FindPath(transform.position, destination);

        // If a valid path was found, add the exact destination as the final step
        if (path != null && path.Count > 0)
        {
            Vector3 finalStep = destination;

            // Only add final step if it's not already close to the last path point
            if (Vector3.Distance(path[^1], finalStep) > 0.05f)
            {
                path.Add(finalStep);
            }

            currentPath = new Queue<Vector3>(path);
        }
        else
        {
            currentPath = new Queue<Vector3>();
            Debug.LogWarning("New current path is empty!");
        }
    }


    private void MoveTowardsNextTileInPath()
    {
        if (currentPath.Count == 0) return;

        Vector3 next = currentPath.Peek();
        // Debug.Log("Next tile goal: " + next);
        RequestMove(next);

        if (IsCloseTo(next))
            currentPath.Dequeue();
    }

    private GameObject GetClosest(List<GameObject> objects)
    {
        GameObject closest = null;
        float shortestDistance = float.MaxValue;

        foreach (GameObject obj in objects)
        {
            if (obj == null) continue;

            float distance = Vector3.Distance(transform.position, obj.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = obj;
            }
        }

        return closest;
    }


    private void ClearOrder()
    {
        currentOrder = null;
        factionWarManager.ClearOrder(currentLord);
    }

    private void CompleteOrder()
    {
        factionWarManager.CompleteOrder(currentOrder);
    }

    private void WaitInFief(SiegableSettlement sc)
    {
        // add to seige parties
        sc.AddParty(selfPresence);
        // set visuals
        selfPresence.WaitInFief();
        // set bool
        isInFief = true;
    }

}


