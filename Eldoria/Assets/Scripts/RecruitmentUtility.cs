using UnityEngine;
public static class RecruitmentUtility
{
    public static bool TryRecruitUnit(SoldierInstance unit, PartyController party, LordProfile lord, RecruitmentSource source)
    {
        if (!lord.TrySpendGold(unit.soldierData.recruitmentCost))
        {
            Debug.Log($"❌ {lord.Lord.UnitName} can't afford {unit.UnitName} ({unit.soldierData.recruitmentCost} gold)");
            return false;
        }

        if (!party.AddUnit(unit))
        {
            Debug.Log($"❌ Failed to add {unit.UnitName} to party");
            return false;
        }

        source.recruitUnit(unit);
        Debug.Log($"✅ {lord.Lord.UnitName} recruited {unit.UnitName} for {unit.soldierData.recruitmentCost} gold");
        return true;
    }
}
