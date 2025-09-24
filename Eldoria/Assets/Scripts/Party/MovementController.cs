using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField]
    private float speed;

    public void MoveTowards(Vector3 targetPosition)
    {
        // Stop when close enough
        if (Vector2.Distance(transform.position, targetPosition) < 0.01f) return;

        float multiplier = MovementCostManager.Instance.GetSpeedMultiplier(transform.position); // should this be target position? 

        float step = speed * multiplier * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);


    }


}
