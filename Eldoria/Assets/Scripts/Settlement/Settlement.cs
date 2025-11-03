using System.Collections.Generic;
using UnityEngine;

public abstract class Settlement : MonoBehaviour, IInteractable
{
    [SerializeField] protected string settlementName;
    [SerializeField] protected int prosperity;

    public string GetSettlementName() => settlementName;
    public int GetProsperity() => prosperity;

    public Faction GetFaction() =>
        TerritoryManager.Instance.GetLordOf(this)?.Faction;

    public LordProfile GetOwner() =>
        TerritoryManager.Instance.GetLordOf(this);

    public abstract List<InteractionOption> GetInteractionOptions();
    public abstract void Interact();
}
