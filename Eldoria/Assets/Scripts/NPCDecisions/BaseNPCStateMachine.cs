using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    [SerializeField] protected float maxChaseDistance = 10f;
    [SerializeField] protected float ignoreTime = 10f;
    protected Vector3 chaseOrigin;
    private Dictionary<PartyPresence, float> ignoreUntil = new();


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
        if (!TickManager.Instance.IsTicking) return;

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

    // for actions that need to be taken when changing states
    protected virtual void ExecuteTransitionActions()
    {
        if (currentState == NPCState.Chasing)
        {
            chaseOrigin = transform.position;
            Debug.Log($"{selfPresence.Lord.Lord.UnitName} started chasing from {chaseOrigin}");
        }
    }

    protected Vector3 targetDirection;

    protected void FleeFromGroup()
    {
        if (selfPresence.IsInFief()) return; // don't flee if in a fief
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
        if (selfPresence.IsInFief()) return; // don't chase if in fief
        if (nearbyEnemies.Count == 0)
        {
            currentState = NPCState.Idle;
            return;
        }

        PartyPresence closest = null;
        float closestDistance = float.MaxValue;

        foreach (var enemy in nearbyEnemies)
        {
            if (enemy == null)
            {
                nearbyEnemies.Remove(enemy);
                continue;
            }
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = enemy;
            }
        }

        if (closest != null)
        {
            // check chase distance
            float chaseDistance = Vector3.Distance(chaseOrigin, transform.position);
            if (chaseDistance > maxChaseDistance)
            {
                Debug.Log($"{selfPresence.Lord.Lord.UnitName} abandoning chase after {chaseDistance:F1} units.");
                ignoreUntil[closest] = Time.time + 10f;
                currentState = NPCState.Idle;
                return;
            }
            targetDirection = (closest.transform.position);
            RequestMove(targetDirection);
        }
        if (Vector2.Distance(closest.transform.position, transform.position) < 0.01f)
        {
            // check not in settlement or something

            // attempt attack
            AttackEnemy(closest);






        }
    }

    protected void AttackEnemy(PartyPresence enemy)
    {
        // check still interactable
        int interactableLayer = LayerMask.NameToLayer("Interactable");
        int playerLayer = LayerMask.NameToLayer("Player");
        if (enemy.gameObject.layer == interactableLayer || enemy.gameObject.layer == playerLayer)
        {

            Debug.Log(selfPresence.Lord.Lord.UnitName + "is attacking " + enemy.Lord.Lord.UnitName);

            if (enemy.gameObject == GameManager.Instance.player)
            {
                Debug.Log("Attacking player");
                CombatSimulator.StartBattle(gameObject.transform.position, selfPresence.Lord.Faction, enemy.GetComponent<PartyPresence>().Lord.Faction, false);


            }
            else
            {
                CombatSimulator.StartBattle(gameObject.transform.position, selfPresence.Lord.Faction, enemy.GetComponent<PartyPresence>().Lord.Faction);
            }
        }
    }


    public void OnThreatDetected(PartyPresence enemyPresence)
    {
        // ignore if i already chased them forever. 
        if (ignoreUntil.TryGetValue(enemyPresence, out float until) && Time.time < until)
        {
            Debug.Log($"{name} ignoring {enemyPresence.Lord.Lord.UnitName} until {until}");
            return;
        }
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
        else
        {
            currentState = NPCState.Chasing;
        }
    }

    protected void RequestMove(Vector3 target)
    {
        movementController.MoveTowards(target, selfPresence.Speed);
    }

    protected bool IsCloseTo(Vector3 target, float threshold = 0.05f)
    {
        return Vector3.Distance(transform.position, target) < threshold;
    }


}

