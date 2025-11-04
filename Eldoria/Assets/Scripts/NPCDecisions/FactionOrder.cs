using UnityEngine;

public enum FactionOrderType
{
    Patrol,
    Defend,
    Attack
}

public class FactionOrder
{
    public FactionOrderType Type { get; private set; }
    public Vector3 TargetLocation { get; private set; }
    public IInteractable TargetObject { get; private set; } // Optional — used for Attack orders
    public float Priority { get; private set; }

    public FactionOrder(FactionOrderType type, Vector3 location, IInteractable targetObject = null, float priority = 1f)
    {
        Type = type;
        TargetLocation = location;
        TargetObject = targetObject;
        Priority = priority;
    }

    public bool HasTargetObject => TargetObject != null;

    public override string ToString()
    {
        string target = HasTargetObject ? TargetObject.ToString() : "None";
        return $"Order: {Type}, Location: {TargetLocation}, Target: {target}, Priority: {Priority}";
    }
}
