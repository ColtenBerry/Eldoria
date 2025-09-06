using UnityEngine;

public class Settlement : MonoBehaviour, IInteractable
{
    [SerializeField]
    private string faction;
    [SerializeField]
    private string settlementName;
    [SerializeField]
    private int prosperity;

    public void Interact()
    {
        Debug.Log("Attempt interaction with " + settlementName);
    }
}
