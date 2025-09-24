using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    public InventoryItem bread;
    public InventoryItem ham;
    public Inventory inventory;
    [SerializeField]
    private PlayerInventoryManager playerInventoryManager;

    // public float moveSpeed = 5f;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool isFolowingInteractable = false;
    private Transform targetInteractable;
    private IInteractable pendingInteraction;
    private MovementController movementController;

    private void OnEnable()
    {
        InputEvents.OnInputPosition += SetTargetPosition;
        InputEvents.OnInteractableSelected += SetTargetInteractable;
    }

    private void OnDisable()
    {
        InputEvents.OnInputPosition -= SetTargetPosition;
        InputEvents.OnInteractableSelected -= SetTargetInteractable;
    }

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
        inventory = new Inventory();
        playerInventoryManager.Initialize(inventory);
        playerInventoryManager.AddItem(bread);
        playerInventoryManager.AddItem(ham, 5);
    }

    void Update()
    {
        // Move toward target
        if (isMoving)
        {
            if (isFolowingInteractable)
            {
                targetPosition = targetInteractable.position;
            }
            // transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            RequestMove(targetPosition);

            // Stop when close enough
            if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMoving = false;

                // Attempt interaction if necessary
                if (isFolowingInteractable)
                {
                    isFolowingInteractable = false;
                    pendingInteraction.Interact();
                }
            }

        }
    }

    private void SetTargetPosition(Vector2 selectedPos)
    {
        isFolowingInteractable = false;
        isMoving = true;
        targetPosition = selectedPos;
    }
    private void SetTargetInteractable(IInteractable interactable)
    {
        isFolowingInteractable = true;
        isMoving = true;
        pendingInteraction = interactable;
        targetInteractable = (interactable as MonoBehaviour)?.transform;
    }

    private void RequestMove(Vector3 targetPosition)
    {
        movementController.MoveTowards(targetPosition);
    }
}


