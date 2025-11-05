using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum NPCState { Idle, Chasing, Fleeing }

[RequireComponent(typeof(PartyController))]
public abstract class BaseNPCStateMachine : MonoBehaviour
{
    public NPCState currentState;
    protected NPCState previousState;
    protected MovementController movementController;
    protected PartyPresence selfPresence;
    [SerializeField] protected float strengthPercent = 0.25f; // difference in strength to change behavior



    protected virtual void Awake()
    {
        movementController = GetComponent<MovementController>();
        if (movementController == null) Debug.LogError("Movement Controller not found on " + gameObject.name);

        selfPresence = GetComponent<PartyPresence>();
        if (selfPresence == null) Debug.LogError("PartyPresence not found on " + gameObject.name);
    }

    public List<PartyPresence> nearbyEnemies = new();
    public List<PartyPresence> nearbyAllies = new();
    protected virtual void FixedUpdate()
    {
        if (InputGate.IsUIBlockingGameInput) return;

        EvaluateGroupThreat();

        if (currentState != previousState)
        {
            ExecuteTransitionActions();
        }

        switch (currentState)
        {
            case NPCState.Idle:
                SetIdleBehavior();
                break;
            case NPCState.Fleeing:
                FleeFromGroup();
                break;
            case NPCState.Chasing:
                ChaseClosestEnemy();
                break;
        }
        previousState = currentState;


    }

    protected abstract void SetIdleBehavior();
    protected abstract void ExecuteTransitionActions(); // for actions that need to be taken when changing states

    protected Vector3 targetDirection;

    protected void FleeFromGroup()
    {
        if (nearbyEnemies.Count == 0)
        {
            currentState = NPCState.Idle;
            return;
        }

        Vector3 threatCenter = Vector3.zero;
        foreach (var enemy in nearbyEnemies)
            threatCenter += enemy.transform.position;

        threatCenter /= nearbyEnemies.Count;

        targetDirection = (transform.position - threatCenter).normalized;
        RequestMove(targetDirection + transform.position);
    }


    protected void ChaseClosestEnemy()
    {
        if (nearbyEnemies.Count == 0)
        {
            currentState = NPCState.Idle;
            return;
        }

        PartyPresence closest = null;
        float closestDistance = float.MaxValue;

        foreach (var enemy in nearbyEnemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = enemy;
            }
        }

        if (closest != null)
        {
            targetDirection = (closest.transform.position);
            RequestMove(targetDirection);
        }
        if (Vector2.Distance(closest.transform.position, transform.position) < 0.01f)
        {
            // attempt attack

            if (closest.gameObject.name == "Player")
            {
                Debug.Log("Attacking Player!!!");
                PartyController personalPartyController = GetComponent<PartyController>();
                if (personalPartyController == null) Debug.LogWarning("Party controller not found");
                CombatSimulator.InitiateCombat(personalPartyController, false);
            }
            else
            {
                Debug.Log(selfPresence.Lord.Lord.UnitName + "is attacking " + closest.Lord.Lord.UnitName);

                PartyController personalPartyController = GetComponent<PartyController>();
                if (personalPartyController == null) Debug.LogWarning("Party controller not found");

                PartyController enemyPartyController = closest.GetComponent<PartyController>();
                if (enemyPartyController == null) Debug.LogWarning("party controller not found");

                CombatSimulator.InitiateCombat(personalPartyController, enemyPartyController);
            }

        }
    }


    public void OnThreatDetected(PartyPresence enemyPresence)
    {
        if (!nearbyEnemies.Contains(enemyPresence))
            nearbyEnemies.Add(enemyPresence);
    }

    public void OnThreatExited(PartyPresence enemyPresence)
    {
        nearbyEnemies.Remove(enemyPresence);
    }

    public void OnFriendDetected(PartyPresence friendPresence)
    {
        if (!nearbyAllies.Contains(friendPresence))
            nearbyAllies.Add(friendPresence);
    }

    public void OnFriendExited(PartyPresence friendPresence)
    {
        nearbyAllies.Remove(friendPresence);
    }


    protected void EvaluateGroupThreat()
    {
        if (nearbyEnemies.Count == 0)
        {
            currentState = NPCState.Idle;
            return;
        }

        float myStrength = selfPresence.GetStrengthEstimate();
        float totalEnemyStrength = nearbyEnemies.Sum(e => e.GetStrengthEstimate());
        float totalFriendStrength = nearbyAllies.Sum(f => f.GetStrengthEstimate());

        float netStrength = (totalFriendStrength + myStrength) - totalEnemyStrength;
        float ratio = -netStrength / Mathf.Max(myStrength, 1f); // negative means we're weaker

        if (ratio > strengthPercent)
            currentState = NPCState.Fleeing;
        else if (ratio < -strengthPercent)
            currentState = NPCState.Chasing;
        else
        {
            currentState = NPCState.Idle;
        }
    }

    protected void RequestMove(Vector3 target)
    {
        movementController.MoveTowards(target);
    }

    protected bool IsCloseTo(Vector3 target, float threshold = 0.05f)
    {
        return Vector3.Distance(transform.position, target) < threshold;
    }


}

