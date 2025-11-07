using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public static class CombatOutcomeProcessor
{
    public static void ApplyCombatResult(CombatResult result, PartyController party1, PartyController party2)
    {
        double SURVIVE_CHANCE = .5;
        ApplyDamage(result.AttackerParty, party1, SURVIVE_CHANCE);
        ApplyDamage(result.DefenderParty, party2, SURVIVE_CHANCE);

        var defeatedParty2 = GetDefeatedUnits(result.DefenderParty);
        RewardExperience(result.AttackerParty, party1, defeatedParty2);

        var defeatedParty1 = GetDefeatedUnits(result.AttackerParty);
        RewardExperience(result.DefenderParty, party2, defeatedParty1);

    }

    private static List<UnitInstance> GetDefeatedUnits(List<UnitInstance> simulatedUnits)
    {
        return simulatedUnits.Where(u => u.Health <= 0).ToList();
    }


    public static List<UnitInstance> ReturnPrisoners(PartyController losingParty)
    {
        return losingParty.PartyMembers;
    }


    private static void ApplyDamage(List<UnitInstance> simulatedUnits, PartyController party, double surviveChance)
    {
        Debug.Log("Applying to Party IDs: " + string.Join(",", party.PartyMembers.Select(u => u.ID)));

        List<UnitInstance> toRemove = new();

        foreach (var simulated in simulatedUnits)
        {
            var actual = party.PartyMembers.Find(u => u.ID == simulated.ID);
            if (actual == null)
            {
                Debug.LogWarning($"Simulated unit ID {simulated.ID} not found. Party has: {string.Join(",", party.PartyMembers.Select(u => u.ID))}");
                continue;
            }

            actual.SetHealth(simulated.Health);

            if (actual.Health == 0)
            {
                if (DecideDeath(surviveChance))
                {
                    toRemove.Add(actual);
                }
                else
                {
                    actual.Heal(1);
                }
            }
        }

        foreach (var unit in toRemove)
        {
            party.RemoveUnit(unit);
        }
    }


    private static bool DecideDeath(double surviveChance)
    {
        System.Random rng = new System.Random();
        return rng.NextDouble() > surviveChance;
    }


    private static void RewardExperience(List<UnitInstance> simulatedUnits, PartyController party, List<UnitInstance> defeatedEnemies)
    {
        int totalXP = defeatedEnemies.Sum(enemy => CalculateXPFromUnit(enemy));
        var survivors = simulatedUnits.Where(u => u.Health > 0).ToList();

        if (survivors.Count == 0) return;

        int xpPerUnit = Mathf.Max(1, totalXP / survivors.Count);
        Debug.Log("XP per unit awarded");

        foreach (var unit in survivors)
        {
            var actual = party.PartyMembers.Find(u => u.ID == unit.ID);
            if (actual != null)
                actual.GainExperience(xpPerUnit);
        }
    }


    private static int CalculateXPFromUnit(UnitInstance enemy)
    {
        int statSum = enemy.Attack + enemy.Defence + enemy.Moral;
        return Mathf.Max(1, statSum / 10); // tweak divisor to scale difficulty
    }





}