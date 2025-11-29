using UnityEngine;

public class SoldierInstance : UnitInstance
{
    public SoldierInstance(UnitData data) : base(data)
    {
    }

    public override UnitInstance Clone()
    {
        return new SoldierInstance(unitData)
        {
            unitName = unitName,
            currentLevel = currentLevel,
            currentExperience = currentExperience,
            experienceToNextLevel = experienceToNextLevel,
            health = health,
            maxHealth = maxHealth,
            attack = attack,
            defence = defence,
            moral = moral,
            idString = idString
        };
    }
}
