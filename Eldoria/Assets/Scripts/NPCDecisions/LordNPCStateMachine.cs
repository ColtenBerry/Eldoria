using System.Collections.Generic;
using System.Linq;
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


public class LordNPCStateMachine : BaseNPCStateMachine
{

    private Queue<Vector3> currentPath = new();

    public FactionOrder currentOrder;
    [SerializeField] private NPCIntent currentIntent;
    [SerializeField] private NPCIntent previousIntent;
    private LordProfile currentLord;


    void Initialize()
    {
        if (currentLord != null) return;
        currentLord = LordRegistry.Instance.GetLordByName(gameObject.name);
        Debug.Log("Current lord is " + currentLord.Lord.UnitName);

    }
    protected override void SetIdleBehavior()
    {
        if (currentOrder == null)
        {
            currentIntent = NPCIntent.None;
            MakeIndividualDecision();
            return;
        }

        switch (currentOrder.Type)
        {
            case FactionOrderType.Patrol:
                currentIntent = NPCIntent.PatrolTerritory;
                Patrol(currentOrder.TargetLocation);
                break;
            case FactionOrderType.Defend:
                currentIntent = ResolveDefendIntent(currentOrder.TargetObject);
                Defend(currentOrder.TargetObject, currentOrder.TargetLocation);
                break;
            case FactionOrderType.Attack:
                currentIntent = ResolveAttackIntent(currentOrder.TargetObject);
                Engage(currentOrder.TargetObject, currentOrder.TargetLocation);
                break;
        }
    }

    private void Patrol(Vector3 location)
    {
        RequestMove(location); // later we will implement a patrol loop
    }

    private void Defend(IInteractable interactableObject, Vector3 targetPosition)
    {
        if (IsCloseTo(targetPosition))
        {
            // engage nearby enemies, wait in territory, etc.

        }
        else RequestMove(targetPosition); // move to spot
    }

    private void Engage(IInteractable interactableObject, Vector3 targetPosition)
    {
        if (IsCloseTo(targetPosition))
        {
            // engage target. Siege castle, raid land, attack party, etc. 

        }
        else MoveTowardsNextTileInPath(); // move to spot
    }

    private void MakeIndividualDecision()
    {
        Initialize();
        if (currentLord == null && true) return;
        // choose to recruit, sit in fief, upgrade fief, patrol lands, etc.
        GameObject objective;
        if (ShouldRecruit())
        {
            currentIntent = NPCIntent.RecruitTroops;
            // get closest owned settlement with recruits
            List<Settlement> ownedSettlements = TerritoryManager.Instance.GetSettlementsOf(currentLord);
            var targetSettlement = ownedSettlements.Where(s =>
        {
            var source = s.GetComponent<RecruitmentSource>();
            return source != null && source.GetRecruitableUnits().Count > 0;
        })
        .OrderBy(s => Vector3.Distance(s.transform.position, transform.position))
        .FirstOrDefault();

            if (targetSettlement == null)
            {
                Debug.LogWarning($"{name} could not find a recruitable settlement.");
                return;
            }

            objective = targetSettlement.gameObject;


        }

        else
        {
            // get owned fief to sit in. 
            currentIntent = NPCIntent.WaitInFief;
            List<Settlement> ownedSettlements = TerritoryManager.Instance.GetSettlementsOf(currentLord);
            Settlement richest = ownedSettlements.OrderByDescending(s => s.GetProsperity()).FirstOrDefault();
            objective = richest.gameObject;
        }

        Vector3 intentLocation = objective.transform.position;
        // check previous intent against current intent
        if (previousIntent != currentIntent)
        {
            currentPath.Clear();

            GeneratePathTO(intentLocation);

            previousIntent = currentIntent;
        }

        if (IsCloseTo(intentLocation))
        {
            // attempt relevant action. 
            switch (currentIntent)
            {
                case NPCIntent.RecruitTroops:
                    Recruit();
                    break;
                case NPCIntent.WaitInFief:
                    WaitInFief();
                    break;
            }
        }
        else
        {
            MoveTowardsNextTileInPath();
        }
    }

    private bool ShouldRecruit()
    {
        // should recruit logic
        return false;
    }

    private void Recruit()
    {
        // code
    }

    public void WaitInFief()
    {
        // code
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


    private void GeneratePathTO(Vector3 destination)
    {
        List<Vector3> path = PathfindingManager.Instance.FindPath(transform.position, destination);
        currentPath = path != null ? new Queue<Vector3>(path) : new Queue<Vector3>();
    }

    private void MoveTowardsNextTileInPath()
    {
        if (currentPath.Count == 0) return;

        Vector3 next = currentPath.Peek();
        Debug.Log("Next tile goal: " + next);
        RequestMove(next);

        if (IsCloseTo(next))
            currentPath.Dequeue();
    }

}


