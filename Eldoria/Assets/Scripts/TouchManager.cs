using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchManager : MonoBehaviour
{
    Vector2 inputPos = new Vector2(0, 0);

    void Update()
    {
        if (InputGate.IsUIBlockingGameInput) return;

        // Detect an input

        bool inputDetected = false;

        // Mouse input (Editor/Desktop)
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            inputDetected = true;
            Debug.Log("detected click: " + inputPos);
        }

        // Touch input (Mobile)
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
            inputPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            inputDetected = true;
        }


        if (inputDetected)
        {
            // Check if object is interactable 
            RaycastHit2D hit = Physics2D.Raycast(inputPos, Vector2.zero);
            if (hit.collider != null)
            {
                Debug.Log($"Clicked or touched: {hit.collider.name}");
                // You can call a method or interface here
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    InputEvents.SelectInteractable(interactable);
                }
            }
            else InputEvents.OnLocationSelected(inputPos);
        }
    }

}

public static class InputGate
{
    public static bool IsUIBlockingGameInput { get; private set; }

    public static Action OnMenuOpened;
    public static Action OnMenuClosed;

    static InputGate()
    {
        OnMenuOpened += () => IsUIBlockingGameInput = true;
        OnMenuClosed += () => IsUIBlockingGameInput = false;
    }
}

public static class InputEvents
{
    public static event Action<Vector2> OnInputPosition;
    public static void OnLocationSelected(Vector2 position)
    {
        // Debug.Log("invoked the input position action");
        OnInputPosition?.Invoke(position);
    }
    public static event Action<IInteractable> onInteractableSelected;
    public static void SelectInteractable(IInteractable interactable)
    {
        onInteractableSelected?.Invoke(interactable);
    }
}