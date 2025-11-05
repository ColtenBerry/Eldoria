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
    private PartyController partyController;

    protected override void Awake()
    {
        base.Awake();
        partyController = GetComponent<PartyController>();
        if (partyController == null)
        {
            Debug.LogWarning("Expecte4d PartyController");
        }
    }


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
        if (currentLord == null) return;
        // choose to recruit, sit in fief, upgrade fief, patrol lands, etc.
        GameObject objective;

        if (ShouldRecruit())
        {
            currentIntent = NPCIntent.RecruitTroops;

            List<Settlement> ownedSettlements = TerritoryManager.Instance.GetSettlementsOf(currentLord);
            List<Settlement> validRecruitmentSources = ownedSettlements.FindAll(settlement =>
        settlement.TryGetComponent<RecruitmentSource>(out var source) &&
        source.GetRecruitableUnits().Count > 0);

            List<GameObject> recruitmentGameObjects = validRecruitmentSources
                .ConvertAll(settlement => settlement.gameObject);

            objective = GetClosest(recruitmentGameObjects);
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

            GeneratePathTo(intentLocation);

            if (previousIntent == NPCIntent.WaitInFief)
            {
                LeaveFief();
            }

            previousIntent = currentIntent;
        }

        if (IsCloseTo(intentLocation))
        {
            // attempt relevant action. 
            switch (currentIntent)
            {
                case NPCIntent.RecruitTroops:
                    Debug.Log("Attempting to get recruitment source");
                    RecruitmentSource source = objective.GetComponent<RecruitmentSource>();
                    if (source == null)
                    {
                        Debug.Log("Recruitment source is null");
                    }
                    Recruit(source);
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

    public void WaitInFief()
    {
        // code
        gameObject.layer = LayerMask.NameToLayer("");
        SetTransparency(gameObject, 0.0f);
    }

    public void LeaveFief()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        SetTransparency(gameObject, 1.0f);
    }

    public void SetTransparency(GameObject root, float alpha)
    {
        foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(includeInactive: true))
        {
            Color color = sr.color;
            color.a = alpha;
            sr.color = color;
        }

        // Handle TextMeshPro components
        foreach (var tmp in root.GetComponentsInChildren<TextMeshPro>(includeInactive: true))
        {
            Color color = tmp.color;
            color.a = alpha;
            tmp.color = color;
        }

        foreach (var tmpUGUI in root.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true))
        {
            Color color = tmpUGUI.color;
            color.a = alpha;
            tmpUGUI.color = color;
        }
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
        Debug.Log("Next tile goal: " + next);
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


