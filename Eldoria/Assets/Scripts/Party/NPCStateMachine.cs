using UnityEngine;
using UnityEngine.UIElements;

public enum NPCState { Wandering, Chasing, Fleeing }

public class NPCStateMachine : MonoBehaviour
{
    public NPCState currentState;
    private MovementController movementController;
    private float timer;
    [SerializeField] private float directionChangeInterval;
    [SerializeField] private float wanderRadius;

    void Awake()
    {
        currentState = NPCState.Wandering;
        movementController = GetComponent<MovementController>();
        if (movementController == null) Debug.LogError("Movement Controller not found on " + gameObject.name);
        origin = transform.position;
    }

    GameObject enemy;
    GameObject target;
    void FixedUpdate()
    {
        if (InputGate.IsUIBlockingGameInput) return;
        switch (currentState)
        {
            case NPCState.Wandering:
                Wander();
                break;
            case NPCState.Fleeing:
                Flee(enemy.transform.position);
                break;
            case NPCState.Chasing:
                Chase(target.transform.position);
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
        targetDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
    }

    void Flee(Vector3 enemyPosition)
    {
        targetDirection = (transform.position - enemyPosition).normalized;
        RequestMove(targetDirection);
    }



    void Chase(Vector3 targetPosition)
    {
        targetDirection = (targetPosition - transform.position).normalized;
        RequestMove(targetDirection);
    }



    public void RequestMove(Vector3 target)
    {
        movementController.MoveTowards(target);
    }

}

