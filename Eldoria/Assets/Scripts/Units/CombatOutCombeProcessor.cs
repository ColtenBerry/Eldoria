using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public static class CombatOutcomeProcessor
{
    public static void ApplyCombatResult(CombatResult result, PartyController party1, PartyController party2)
    {
        double SURVIVE_CHANCE = .5;
        ApplyDamage(result.AttackerParty, party1, SURVIVE_CHANCE);
        ApplyDamage(result.DefenderParty, party2, SURVIVE_CHANCE);
    }

    public static List<UnitInstance> ReturnPrisoners(PartyController losingParty)
    {
        return losingParty.PartyMembers;
    }

    private static void ApplyDamage(List<UnitInstance> simulatedUnits, PartyController party, double surviveChance)
    {
        foreach (var simulated in simulatedUnits)
        {
            Debug.Log($"Looking for simulated ID: {simulated.ID}");
            foreach (var unit in party.PartyMembers)
                Debug.Log($"Party member ID: {unit.ID}");

            var actual = party.PartyMembers.Find(u => u.ID == simulated.ID);
            if (actual == null)
            {
                Debug.LogWarning("error in applying damage. party member not found?");
                continue;
            }
            actual.SetHealth(simulated.Health);
            if (actual.Health == 0)
            {
                // kill unit
                KillUnit(actual, party, surviveChance);
            }
        }
    }

    private static bool DecideDeath(double surviveChance)
    {
        System.Random rng = new System.Random();
        return rng.NextDouble() > surviveChance;
    }


    private static void KillUnit(UnitInstance injuredUnit, PartyController party, double surviveChance)
    {
        // unit dies
        if (DecideDeath(surviveChance))
        {
            if (injuredUnit != null && injuredUnit.Health <= 0)
                party.RemoveUnit(injuredUnit);
        }
        else
        {
            injuredUnit.Heal(1); // give 1 point of health
        }

    }

}