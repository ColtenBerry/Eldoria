using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject
{
    public string unitName;
    [Header("Combat Stats")]
    public int health;
    public int attack;
    public int defence;
    public int moral;
    //public int range;
    [Header("Progression")]
    public int level = 1;
    public int experience = 0;
    public int experienceToNextLevel = 50;
    public List<UnitData> upgradeOptions;

    [Header("Cost")]
    public int recruitmentCost;
    public int upkeepCostPerDay;
    public int upgradeCost;

}
