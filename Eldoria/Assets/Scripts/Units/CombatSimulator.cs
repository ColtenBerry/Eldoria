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
            AttackerParty = party1.Select(u => u.Clone()).ToList(),
            DefenderParty = party2.Select(u => u.Clone()).ToList()
        };


        int safetyCounter = 0;
        while (result.AttackerParty.Count > 0 && result.DefenderParty.Count > 0 && safetyCounter < 1000)
        {
            SimulateRound(result);
            safetyCounter++;
        }

        result.Party1Wins = result.AttackerParty.Count > 0;


        // rewrite the combat result lists so that i get same order units with updated stats
        var simulatedAttackers = result.AttackerParty;
        var simulatedDefenders = result.DefenderParty;

        result.AttackerParty = party1.Select(original =>
        {
            var simulated = simulatedAttackers.FirstOrDefault(sim => sim.ID == original.ID);
            if (simulated != null)
                return simulated;

            var clone = original.Clone();
            clone.SetHealth(0);
            return clone;
        }).ToList();

        result.DefenderParty = party2.Select(original =>
        {
            var simulated = simulatedDefenders.FirstOrDefault(sim => sim.ID == original.ID);
            if (simulated != null)
                return simulated;

            var clone = original.Clone();
            clone.SetHealth(0);
            return clone;
        }).ToList();


        return result;
    }

    private static void SimulateRound(CombatResult result)
    {
        List<UnitInstance> attackers = new(result.AttackerParty);
        List<UnitInstance> defenders = new(result.DefenderParty);
        ShuffleList(attackers);
        ShuffleList(defenders);
        Debug.Log("round called");

        // attackers attack
        for (int i = 0; i < attackers.Count; i++)
        {
            if (defenders.Count == 0) continue;
            int index = i % defenders.Count;
            SimulateAttack(attackers[index], defenders[index]);

        }
        result.AttackerParty.RemoveAll(u => u.Health <= 0);
        result.DefenderParty.RemoveAll(u => u.Health <= 0);


        // defenders attack
        for (int i = 0; i < defenders.Count; i++)
        {
            if (attackers.Count == 0) continue;
            int index = i % attackers.Count;
            SimulateAttack(defenders[index], attackers[index]);

        }

        result.AttackerParty.RemoveAll(u => u.Health <= 0);
        result.DefenderParty.RemoveAll(u => u.Health <= 0);


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
    public List<UnitInstance> AttackerParty;
    public List<UnitInstance> DefenderParty;
    public bool Party1Wins;
}