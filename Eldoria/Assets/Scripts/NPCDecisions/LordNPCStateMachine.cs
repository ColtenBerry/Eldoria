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

    public FactionOrder currentOrder;
    [SerializeField] private NPCIntent currentIntent;
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
        if (Vector3.Distance(transform.position, targetPosition) < .05f)
        {
            // engage nearby enemies, wait in territory, etc.

        }
        else RequestMove(targetPosition); // move to spot
    }

    private void Engage(IInteractable interactableObject, Vector3 targetPosition)
    {
        if (Vector3.Distance(transform.position, targetPosition) < .05f)
        {
            // engage target. Siege castle, raid land, attack party, etc. 

        }
        else RequestMove(targetPosition); // move to spot
    }

    private void MakeIndividualDecision()
    {
        // choose to recruit, sit in fief, upgrade fief, patrol lands, etc.
    }

    protected override void SetTransitionActions()
    {
        // nothing to implement here
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

}


