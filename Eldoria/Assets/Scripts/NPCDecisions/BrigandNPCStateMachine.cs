using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(PartyController))]
public class BrigandNPCStateMachine : BaseNPCStateMachine
{
    private float timer;
    [SerializeField] private float directionChangeInterval;
    [SerializeField] private float wanderRadius;

    [SerializeField][ReadOnly] private Vector3 origin;


    protected override void Awake()
    {
        base.Awake();

        origin = transform.position;
        timer = directionChangeInterval;
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


    void PickNewDirection()
    {
        targetDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
    }

    protected override void SetIdleBehavior()
    {
        Wander();
    }

    protected override void SetTransitionActions()
    {
        if (currentState == NPCState.Idle)
        {
            origin = transform.position;
        }
    }
}

