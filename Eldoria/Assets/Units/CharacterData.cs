using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scriptable Objects/CharacterData")]
public class CharacterData : UnitData
{
    [Header("Stats")]
    public int strength;
    public int agility;
    public int intelligence;
    public int charisma;
    public int endurance;

    [Header("Faction")]
    public string faction;

}
