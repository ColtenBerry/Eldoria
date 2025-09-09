using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public InventoryItem bread;
    public Inventory inventory;
    [SerializeField]
    private PlayerInventoryManager playerInventoryManager;

    public float moveSpeed = 5f;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool isFolowingInteractable = false;
    private Transform targetInteractable;
    private IInteractable pendingInteraction;

    private void OnEnable()
    {
        InputEvents.OnInputPosition += SetTargetPosition;
        InputEvents.onInteractableSelected += SetTargetInteractable;
    }

    private void OnDisable()
    {
        InputEvents.OnInputPosition -= SetTargetPosition;
        InputEvents.onInteractableSelected -= SetTargetInteractable;
    }

    private void Awake()
    {
        inventory = new Inventory();
        playerInventoryManager.Initialize(inventory);
        playerInventoryManager.AddItem(bread);
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
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

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
        pendingInteraction = interactable;
        targetInteractable = (interactable as MonoBehaviour)?.transform;
    }
}


