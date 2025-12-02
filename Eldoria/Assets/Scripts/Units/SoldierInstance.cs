using UnityEngine;

public class SoldierInstance : UnitInstance
{
    public SoldierData soldierData;
    public SoldierInstance(SoldierData data) : base(data)
    {
        soldierData = data;
    }

    public override UnitInstance Clone()
    {
        return new SoldierInstance(soldierData)
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
            idString = idString,
            speed = speed
        };
    }

    protected override void LevelUp()
    {
        currentLevel++;
        //currentExperience = 0;

        if (soldierData.upgradeOptions != null && soldierData.upgradeOptions.Count > 0)
        {
            canUpgrade = true;
            Debug.Log("Can upgrade is true");
        }
    }
    public void ApplyUpgrade(SoldierData newData)
    {
        soldierData = newData;
        unitName = newData.unitName;
        attack = newData.attack;
        defence = newData.defence;
        moral = newData.moral;
        maxHealth = newData.health;
        health = maxHealth;
        // Optionally reset experience or keep it
        currentExperience = 0;
        canUpgrade = false;
        speed = newData.speed;
    }

}
