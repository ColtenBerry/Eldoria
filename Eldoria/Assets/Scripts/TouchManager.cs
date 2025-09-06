using System;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    Vector2 inputPos = new Vector2(0, 0);

    void Update()
    {
        // Detect an input

        bool inputDetected = false;

        // Mouse input (Editor/Desktop)
        if (Input.GetMouseButtonDown(0))
        {
            inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            inputDetected = true;
            // Debug.Log("detected click: " + inputPos);
        }

        // Touch input (Mobile)
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            inputDetected = true;
        }


        if (inputDetected)
        {
            // no matter what, i want to head over here. 
            InputEvents.OnLocationSelected(inputPos);

            // Check if object is interactable 
            RaycastHit2D hit = Physics2D.Raycast(inputPos, Vector2.zero);
            if (hit.collider != null)
            {
                Debug.Log($"Clicked or touched: {hit.collider.name}");
                // You can call a method or interface here
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    InteractionManager.RequestInteraction(interactable);
                }
            }


        }
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
}