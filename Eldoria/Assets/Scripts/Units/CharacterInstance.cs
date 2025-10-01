using UnityEngine;

[System.Serializable]
public class CharacterInstance : UnitInstance
{
    [SerializeField]
    private int strength;
    [SerializeField]
    private int agility;
    [SerializeField]
    private int intelligence;
    [SerializeField]
    private int charisma;
    [SerializeField]
    private int endurance;

    public CharacterInstance(CharacterData data) : base(data)
    {
        strength = data.strength;
        agility = data.agility;
        intelligence = data.intelligence;
        charisma = data.charisma;
        endurance = data.endurance;
    }

    protected override void LevelUp()
    {
        base.LevelUp();
        // Custom stat scaling logic
        strength += 1;
        agility += 1;
        intelligence += 1;
        charisma += 1;
        endurance += 1;
    }
}
