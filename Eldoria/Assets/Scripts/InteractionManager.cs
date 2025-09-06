using UnityEngine;

public static class InteractionManager
{
    private static IInteractable pendingInteraction;
    private static Transform targetTransform;

    public static void RequestInteraction(IInteractable target)
    {
        pendingInteraction = target;
        targetTransform = (target as MonoBehaviour)?.transform;
        InputEvents.OnLocationSelected(targetTransform.position); // have the player move towards the desired position
    }


    private static readonly float RANGE = .05f;
    public static void TryInteract(Vector2 playerPosition)
    {
        if (pendingInteraction == null || targetTransform == null) return;
        float distance = Vector2.Distance(playerPosition, targetTransform.position);

        if (distance <= RANGE)
        {
            pendingInteraction.Interact();
            pendingInteraction = null;
            targetTransform = null;
        }
    }

}
