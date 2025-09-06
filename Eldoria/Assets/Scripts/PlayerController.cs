using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 5f;
    private Vector2 targetPosition;
    private bool isMoving = false;

    private void OnEnable()
    {
        InputEvents.OnInputPosition += SetTargetPosition;
    }

    private void OnDisable()
    {
        InputEvents.OnInputPosition -= SetTargetPosition;
    }

    void Update()
    {
        // Move toward target
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Stop when close enough
            if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;
            }
            InteractionManager.TryInteract(transform.position);
        }
    }

    private void SetTargetPosition(Vector2 selectedPos)
    {
        isMoving = true;
        targetPosition = selectedPos;
    }
}


