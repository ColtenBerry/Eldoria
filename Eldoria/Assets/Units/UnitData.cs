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

    [Header("Campaign Stats")]
    public float speed = 1.0f;
    //public int range;
    [Header("Progression")]
    public int level = 1;
    public int experience = 0;
    public int experienceToNextLevel = 50;

    [Header("Cost")]
    public int recruitmentCost;
    public int upkeepCostPerWeek;
    public int upgradeCost;

    public int foodConsumptionPerDay = 1; // maybe monsters eat more. idk, just an idea but i can change this when i want

    [Header("Image")]
    public Sprite sprite;

}
