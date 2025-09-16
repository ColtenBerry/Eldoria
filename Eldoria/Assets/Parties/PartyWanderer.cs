using UnityEngine;

public class PartyWanderer : MonoBehaviour
{
    [Header("Wander Settings")]
    public float moveSpeed = 2f;
    public float wanderRadius = 10f;
    public float directionChangeInterval = 3f;

    private Vector3 origin;
    private Vector3 targetDirection;
    private float timer;

    void Start()
    {
        origin = transform.position;
        PickNewDirection();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            PickNewDirection();
            timer = directionChangeInterval;
        }

        Vector3 newPosition = transform.position + targetDirection * moveSpeed * Time.deltaTime;

        // Clamp movement within wanderRadius
        if (Vector3.Distance(origin, newPosition) <= wanderRadius)
        {
            transform.position = newPosition;
        }
        else
        {
            PickNewDirection(); // bounce back if out of bounds
        }
    }

    void PickNewDirection()
    {
        targetDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
    }
}
