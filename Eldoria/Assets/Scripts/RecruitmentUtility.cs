using UnityEngine;
public static class RecruitmentUtility
{
    public static bool TryRecruitUnit(UnitInstance unit, PartyController party, LordProfile lord, RecruitmentSource source)
    {
        if (!lord.TrySpendGold(unit.baseData.recruitmentCost))
        {
            Debug.Log($"❌ {lord.Lord.UnitName} can't afford {unit.UnitName} ({unit.baseData.recruitmentCost} gold)");
            return false;
        }

        if (!party.AddUnit(unit))
        {
            Debug.Log($"❌ Failed to add {unit.UnitName} to party");
            return false;
        }

        source.recruitUnit(unit);
        Debug.Log($"✅ {lord.Lord.UnitName} recruited {unit.UnitName} for {unit.baseData.recruitmentCost} gold");
        return true;
    }
}
