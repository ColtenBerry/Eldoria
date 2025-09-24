using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField]
    private float speed;

    public void MoveTowards(Vector3 targetPosition)
    {
        // Stop when close enough
        if (Vector2.Distance(transform.position, targetPosition) >= 0.01f)
        {
            Debug.Log("speed: " + speed);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }

    }


}
