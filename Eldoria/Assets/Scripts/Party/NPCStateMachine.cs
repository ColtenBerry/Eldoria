using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum NPCState { Wandering, Chasing, Fleeing }

[RequireComponent(typeof(PartyController))]
public class NPCStateMachine : MonoBehaviour
{
    public NPCState currentState;
    private MovementController movementController;
    private PartyPresence selfPresence;
    private float timer;
    [SerializeField] private float directionChangeInterval;
    [SerializeField] private float wanderRadius;

    [SerializeField] private float strengthPercent = 0.25f; // difference in strength to change behavior



    void Awake()
    {
        currentState = NPCState.Wandering;
        movementController = GetComponent<MovementController>();
        if (movementController == null) Debug.LogError("Movement Controller not found on " + gameObject.name);

        selfPresence = GetComponent<PartyPresence>();
        if (selfPresence == null) Debug.LogError("PartyPresence not found on " + gameObject.name);

        origin = transform.position;
    }

    public List<PartyPresence> nearbyEnemies = new();
    public List<PartyPresence> nearbyAllies = new();
    void FixedUpdate()
    {
        if (InputGate.IsUIBlockingGameInput) return;

        EvaluateGroupThreat();


        switch (currentState)
        {
            case NPCState.Wandering:
                Wander();
                break;
            case NPCState.Fleeing:
                FleeFromGroup();
                break;
            case NPCState.Chasing:
                ChaseClosestEnemy();
                break;
        }


    }



    void Wander()
    {
        timer -= Time.fixedDeltaTime;

        if (timer <= 0f)
        {
            PickNewDirection();
            timer = directionChangeInterval;
        }

        Vector3 newPosition = transform.position + targetDirection;

        // Clamp movement within wanderRadius
        if (Vector3.Distance(origin, newPosition) <= wanderRadius)
        {
            //  transform.position = newPosition;
            RequestMove(newPosition);

        }
        else
        {
            PickNewDirection(); // bounce back if out of bounds
        }
    }

    private Vector3 targetDirection;
    private Vector3 origin;


    void PickNewDirection()
    {
        targetDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
    }

    void FleeFromGroup()
    {
        if (nearbyEnemies.Count == 0)
        {
            currentState = NPCState.Wandering;
            origin = transform.position;
            return;
        }

        Vector3 threatCenter = Vector3.zero;
        foreach (var enemy in nearbyEnemies)
            threatCenter += enemy.transform.position;

        threatCenter /= nearbyEnemies.Count;

        targetDirection = (transform.position - threatCenter).normalized;
        RequestMove(targetDirection + transform.position);
    }


    void ChaseClosestEnemy()
    {
        if (nearbyEnemies.Count == 0)
        {
            currentState = NPCState.Wandering;
            origin = transform.position;
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
                Debug.Log(selfPresence.Lord.UnitName + "is attacking " + closest.Lord.UnitName);

                PartyController personalPartyController = GetComponent<PartyController>();
                if (personalPartyController == null) Debug.LogWarning("Party controller not found");

                PartyController enemyPartyController = GetComponent<PartyController>();
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


    private void EvaluateGroupThreat()
    {
        if (nearbyEnemies.Count == 0)
        {
            currentState = NPCState.Wandering;
            origin = transform.position;
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
            currentState = NPCState.Wandering;
            origin = transform.position;
        }
    }

    public void RequestMove(Vector3 target)
    {
        movementController.MoveTowards(target);
    }

}

