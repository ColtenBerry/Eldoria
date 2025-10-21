using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject
{
    public string unitName;
    [Header("Combat Stats")]
    public int powerStat; //Will be replaced with attack, health, and defence if combat is upgraded
    public int health;
    public int attack;
    public int defence;
    public int moral;
    //public int range;
    [Header("Progression")]
    public int level;
    public int experience;
    public int experienceToNextLevel;
}
