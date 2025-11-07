using System.Collections.Generic;
using UnityEngine;

public abstract class Settlement : MonoBehaviour, IInteractable
{
    protected string settlementName;
    [SerializeField] protected int prosperity;


    public virtual void Start()
    {
        settlementName = gameObject.name;
    }

    public string GetSettlementName() => settlementName;
    public int GetProsperity() => prosperity;

    public Faction GetFaction() =>
        TerritoryManager.Instance.GetLordOf(this)?.Faction;

    public LordProfile GetOwner() =>
        TerritoryManager.Instance.GetLordOf(this);

    public abstract void ApplyVisuals();

    public abstract List<InteractionOption> GetInteractionOptions();
    public abstract void Interact();
}
