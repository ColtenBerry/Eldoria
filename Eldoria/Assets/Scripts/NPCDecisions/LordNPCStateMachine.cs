using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    protected override void SetIdleBehavior()
    {
        if (currentOrder != null)
        {
            EvaluateFactionOrder();
        }
        else
        {
            EvaluateLocalIntent();
        }
        Debug.Log("execute if close");
        ExecuteIntentIfClose();
    }


    private List<Settlement> unPatrolledLands = new();

    private GameObject previousObjective;
    private GameObject objective;

    private void EvaluateFactionOrder()
    {
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

            // fix: remember the objective so future checks see that we've already applied it
            previousObjective = objective;

            if (wasWaiting && partyPresence != null)
                partyPresence.LeaveFief();
        }
        Debug.Log("Intent: " + currentIntent);
        Debug.Log("Target location: " + currentOrder.TargetLocation);
        Debug.Log("Target object: " + (currentOrder.TargetObject as MonoBehaviour)?.name);

    }

    private void EvaluateLocalIntent()
    {
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

            if (previousIntent == NPCIntent.WaitInFief)
            {
                partyPresence.LeaveFief();
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
                    partyPresence.WaitInFief();
                    break;
                case NPCIntent.PatrolTerritory:
                    var settlement = objective.GetComponent<Settlement>();
                    PatrolLand(settlement);
                    break;
                case NPCIntent.SiegeCastle:
                    Castle castle = objective.GetComponent<Castle>();
                    if (castle != null)
                    {
                        Debug.Log("npc intent thing reached on castle: " + castle.name);
                        SiegeCastle(castle);
                    }
                    else
                    {
                        Debug.LogWarning("previousobjective is not a castle");
                    }

                    break;
                default:
                    Debug.Log("something is wrong. default hit");
                    break;

            }
        }
        else
        {
            MoveTowardsNextTileInPath();
            Debug.Log("Current path count: " + currentPath.Count);
        }
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

    private void ResetPatrolledLands()
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
                source.recruitUnit(unit); // remove from source
            }
        }
    }

    private void SiegeCastle(Castle castle)
    {
        Debug.Log("lordnpcstatemachine sieging castle name: " + castle.name);
        castle.StartSiege(partyController, false);
        factionWarManager.ClearOrder(currentLord);
        currentOrder = null;
    }
    protected override void ExecuteTransitionActions()
    {
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

}


