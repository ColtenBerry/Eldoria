using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting;
using UnityEngine;

public static class CombatSimulator
{
    public static CombatResult SimulateBattle(List<UnitInstance> party1, List<UnitInstance> party2)
    {
        CombatResult result = new CombatResult
        {
            ActiveParty1 = new List<UnitInstance>(party1),
            ActiveParty2 = new List<UnitInstance>(party2),
            InjuredParty1 = new List<UnitInstance>(),
            InjuredParty2 = new List<UnitInstance>(),
            DeadParty1 = new(),
            DeadParty2 = new()
        };


        int safetyCounter = 0;
        while (result.ActiveParty1.Count > 0 && result.ActiveParty2.Count > 0 && safetyCounter < 1000)
        {
            SimulateRound(result);
            safetyCounter++;
        }

        result.Party1Wins = result.ActiveParty1.Count > 0;

        return result;
    }

    private static void SimulateRound(CombatResult result)
    {
        List<UnitInstance> attackers = new List<UnitInstance>(result.ActiveParty1);
        List<UnitInstance> defenders = new List<UnitInstance>(result.ActiveParty2);
        ShuffleList<UnitInstance>(attackers);
        ShuffleList<UnitInstance>(defenders);

        // attackers attack
        for (int i = 0; i < attackers.Count; i++)
        {
            if (defenders.Count == 0) continue;
            int index = i % defenders.Count;
            SimulateAttack(attackers[index], defenders[index]);
            if (defenders[index].Health <= 0) defenders.Remove(defenders[index]);

        }

        // defenders attack
        for (int i = 0; i < defenders.Count; i++)
        {
            if (attackers.Count == 0) continue;
            int index = i % attackers.Count;
            SimulateAttack(defenders[index], attackers[index]);
            if (attackers[index].Health <= 0) attackers.Remove(attackers[index]);

        }
        PurgeDeadUnits(result);

    }
    private static void PurgeDeadUnits(CombatResult result)
    {
        result.InjuredParty1.AddRange(result.ActiveParty1.Where(u => u.Health <= 0));
        result.InjuredParty2.AddRange(result.ActiveParty2.Where(u => u.Health <= 0));

        result.ActiveParty1.RemoveAll(u => u.Health <= 0);
        result.ActiveParty2.RemoveAll(u => u.Health <= 0);
    }


    private static void SimulateAttack(UnitInstance attacker, UnitInstance defender)
    {
        // defender stats
        int defence = defender.Defence;
        float defMoral = defender.Moral - 50;
        float totalDefence = ApplyBonus(defence, defMoral);

        // attacker stats
        int attack = attacker.Attack;
        int attMoral = attacker.Moral - 50;
        float totalAttack = ApplyBonus(attack, attMoral);

        // get damage
        float damage = totalAttack - totalDefence;
        int damageRounded = Mathf.Max(0, (int)Math.Round(damage));

        defender.TakeDamage(damageRounded);
    }

    private static float ApplyBonus(int baseStat, float bonus)
    {
        return baseStat + (baseStat * (bonus / 100));
    }

    private static void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]); // cool way to swap without a temp value
        }
    }

    // private static void DecideDeathOrInjured(UnitInstance unit, CombatResult result)
    // {
    //     System.Random rng = new System.Random();

    //     if (rng.Next(2) == 0)
    //     {
    //         if (!result.InjuredParty1.Remove(unit)) Debug.LogWarning("attempting to kill unit not injured?");
    //         result.DeadParty1.Add(unit);
    //     }
    //     else
    //     {
    //         unit.Heal(1); // give 1 point of health.
    //     }
    // }


}

public class CombatResult
{
    public List<UnitInstance> ActiveParty1;
    public List<UnitInstance> ActiveParty2;
    public List<UnitInstance> InjuredParty1;
    public List<UnitInstance> InjuredParty2;
    public List<UnitInstance> DeadParty1;
    public List<UnitInstance> DeadParty2;
    public bool Party1Wins;
}