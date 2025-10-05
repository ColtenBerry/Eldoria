using System.Collections.Generic;
using UnityEngine;

public abstract class Settlement : MonoBehaviour, IInteractable
{
    [SerializeField]
    protected string faction;
    public string GetFaction() => faction;
    [SerializeField]
    protected string settlementName;
    public string GetSettlementName() => settlementName;
    [SerializeField]
    protected int prosperity;
    public int GetProsperity() => prosperity;

    public abstract List<InteractionOption> GetInteractionOptions();

    public abstract void Interact();

}
