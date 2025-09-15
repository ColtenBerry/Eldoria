using UnityEngine;

public class Village : Settlement
{
    public PartyController controller;
    public UnitData militiaTroop;
    public override void Interact()
    {
        Debug.Log("Attempting interaction with " + settlementName);
        controller.AddUnit(militiaTroop);

    }
}