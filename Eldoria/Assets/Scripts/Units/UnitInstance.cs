
using System;
using UnityEngine;

[System.Serializable]
public class UnitInstance
{
    public UnitData baseData;

    [SerializeField]
    private string unitName;
    [SerializeField]
    private int currentExperience;
    [SerializeField]
    private int currentLevel;
    [SerializeField]
    private int health;
    [SerializeField]
    private int maxHealth;
    [SerializeField]
    private int attack;
    [SerializeField]
    private int defence;
    [SerializeField]
    private int moral;
    [SerializeField]
    private int experienceToNextLevel;


    public string UnitName => unitName; // effectively makes this readonly but public? 
    public int CurrentLevel => currentLevel;
    public int CurrentExperience => currentExperience;
    public int ExperienceToNextLevel => experienceToNextLevel;
    public int Health => health;
    public int MaxHealth => maxHealth;
    public int Attack => attack;
    public int Defence => defence;
    public int Moral => moral;



    public UnitInstance(UnitData data)
    {
        baseData = data;
        unitName = data.unitName;
        currentLevel = data.level;
        currentExperience = data.experience;
        experienceToNextLevel = data.experienceToNextLevel;
        health = data.health;
        maxHealth = data.health;
        attack = data.attack;
        defence = data.defence;
        moral = data.moral;
    }

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

        // kill unit? 
    }

    protected virtual void LevelUp()
    {
        currentLevel++;
        currentExperience = 0;
        // Optionally scale powerStat or other shared stats
    }
}
