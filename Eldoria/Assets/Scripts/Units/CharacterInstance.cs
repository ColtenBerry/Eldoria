using UnityEngine;

[System.Serializable]
public class CharacterInstance : UnitInstance
{
    [Header("Stats")]
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

    public int Strength => strength;
    public int Agility => agility;
    public int Intelligence => intelligence;
    public int Charisma => charisma;
    public int Endurance => endurance;
    public CharacterData characterData;


    public CharacterInstance(CharacterData data) : base(data)
    {
        this.characterData = data;
        strength = data.strength;
        agility = data.agility;
        intelligence = data.intelligence;
        charisma = data.charisma;
        endurance = data.endurance;

    }

    protected override void LevelUp()
    {
        // Custom stat scaling logic
        strength += 1;
        agility += 1;
        intelligence += 1;
        charisma += 1;
        endurance += 1;
    }

    public override UnitInstance Clone()
    {
        return new CharacterInstance(characterData)
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
            speed = speed,
            idString = idString
        };
    }

}
