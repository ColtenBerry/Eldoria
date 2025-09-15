using UnityEngine;

public abstract class Settlement : MonoBehaviour, IInteractable
{
    [SerializeField]
    protected string faction;
    [SerializeField]
    protected string settlementName;
    [SerializeField]
    protected int prosperity;

    public abstract void Interact();

}
