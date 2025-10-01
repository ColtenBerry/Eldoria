
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
    private int powerStat;
    [SerializeField]
    private int experienceToNextLevel;


    public string UnitName => unitName; // effectively makes this readonly but public? 
    public int CurrentLevel => currentLevel;
    public int CurrentExperience => currentExperience;
    public int PowerStat => powerStat;


    public UnitInstance(UnitData data)
    {
        baseData = data;
        unitName = data.unitName;
        currentLevel = data.level;
        currentExperience = data.experience;
        experienceToNextLevel = data.experienceToNextLevel;
        powerStat = data.powerStat;
    }

    public virtual void GainExperience(int amount)
    {
        currentExperience += amount;
        if (currentExperience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }

    protected virtual void LevelUp()
    {
        currentLevel++;
        currentExperience = 0;
        // Optionally scale powerStat or other shared stats
    }
}
