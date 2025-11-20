using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
    public Inventory inventory;
    [SerializeField]
    private InventoryManager playerInventoryManager;


    // public float moveSpeed = 5f;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool isFolowingInteractable = false;
    private Transform targetInteractable;
    private GameObject pendingInteraction;
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
    }

    void Update()
    {
        if (InputGate.IsUIBlockingGameInput) return;
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
                    // pendingInteraction.Interact();
                    List<InteractionOption> options = GetAllInteractables(pendingInteraction);
                    // display the options
                    UIManager.Instance.OpenInteractionMenu(options);
                }
            }

        }
    }

    private List<InteractionOption> GetAllInteractables(GameObject interactable)
    {
        var interactables = interactable.GetComponents<IInteractable>();
        List<InteractionOption> allOptions = new();

        foreach (var i in interactables)
        {
            allOptions.AddRange(i.GetInteractionOptions());
        }
        return allOptions;
    }

    private void SetTargetPosition(Vector2 selectedPos)
    {
        isFolowingInteractable = false;
        isMoving = true;
        targetPosition = selectedPos;
    }
    private void SetTargetInteractable(GameObject interactable)
    {
        isFolowingInteractable = true;
        isMoving = true;
        pendingInteraction = interactable;
        targetInteractable = interactable.transform;
    }

    private void RequestMove(Vector3 targetPosition)
    {
        movementController.MoveTowards(targetPosition);
    }
}


