
using System;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public abstract class UnitInstance
{
    public UnitData unitData;

    [SerializeField]
    protected string unitName;
    [SerializeField]
    protected int currentExperience;
    [SerializeField]
    protected int currentLevel;
    [SerializeField]
    protected int health;
    [SerializeField]
    protected int maxHealth;
    [SerializeField]
    protected int attack;
    [SerializeField]
    protected int defence;
    [SerializeField]
    protected int moral;

    [SerializeField]
    protected float speed;
    [SerializeField]
    protected int experienceToNextLevel;
    [SerializeField] protected bool canUpgrade;


    public string UnitName => unitName; // effectively makes this readonly but public? 
    public int CurrentLevel => currentLevel;
    public int CurrentExperience => currentExperience;
    public int ExperienceToNextLevel => experienceToNextLevel;
    public int Health => health;
    public int MaxHealth => maxHealth;
    public int Attack => attack;
    public int Defence => defence;
    public int Moral => moral;
    public float Speed => speed;

    public bool CanUpgrade => canUpgrade;


    [SerializeField] protected string idString;

    public Guid ID
    {
        get => Guid.Parse(idString);
        private set => idString = value.ToString();
    }

    // public Guid ID { get; private set; } = Guid.NewGuid();



    public UnitInstance(UnitData data)
    {
        ID = Guid.NewGuid();
        unitData = data;
        unitName = data.unitName;
        currentLevel = data.level;
        currentExperience = data.experience;
        experienceToNextLevel = data.experienceToNextLevel;
        health = data.health;
        maxHealth = data.health;
        attack = data.attack;
        defence = data.defence;
        moral = data.moral;
        canUpgrade = false;
        speed = data.speed;
    }

    public abstract UnitInstance Clone();

    // public UnitInstance Clone()
    // {
    //     return new UnitInstance(baseData)
    //     {
    //         unitName = unitName,
    //         currentLevel = currentLevel,
    //         currentExperience = currentExperience,
    //         experienceToNextLevel = experienceToNextLevel,
    //         health = health,
    //         maxHealth = maxHealth,
    //         attack = attack,
    //         defence = defence,
    //         moral = moral,
    //         idString = idString
    //     };
    // }



    public virtual void GainExperience(int amount)
    {
        currentExperience += amount;
        if (currentExperience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }

    public virtual void TakeDamage(int damageAmount)
    {
        if (damageAmount < 0) return; // prevents odd scenarios where taking damage actually restores health
        health = health - damageAmount;
        if (health < 0) health = 0;
    }

    public virtual void SetHealth(int value)
    {
        health = value;
    }

    public virtual void Heal(int amount)
    {
        if (amount < 0) return; // prevents any edge cases
        health = health + amount;
        if (health > maxHealth) health = maxHealth;
    }

    protected abstract void LevelUp();
    public void ApplyUpgrade(UnitData newData)
    {
        unitData = newData;
        unitName = newData.unitName;
        attack = newData.attack;
        defence = newData.defence;
        moral = newData.moral;
        maxHealth = newData.health;
        health = maxHealth;
        // Optionally reset experience or keep it
        currentExperience = 0;
        canUpgrade = false;
    }

}
