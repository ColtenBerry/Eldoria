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



}