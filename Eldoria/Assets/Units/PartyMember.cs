using UnityEngine;

[System.Serializable]
public class PartyMember
{
    public UnitData unitData;
    public int currentPower;

    public PartyMember(UnitData unitData)
    {
        this.unitData = unitData;
        currentPower = unitData.powerStat;
    }

    public void ModifyPower(int delta)
    {
        currentPower = Mathf.Max(currentPower + delta, 0); // will never drop below 0
    }

    public bool IsAcgtive => currentPower > 0;
}
