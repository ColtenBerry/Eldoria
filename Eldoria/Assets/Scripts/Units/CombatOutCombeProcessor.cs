using System.Collections.Generic;
using UnityEngine;

public static class CombatOutCombeProcessor
{
    public static void ApplyCombatResult(CombatResult result, PartyController party1, PartyController party2)
    {
        ApplyDamage(result.ActiveParty1, party1);
        ApplyDamage(result.ActiveParty2, party2);

        double SURVIVE_CHANCE = .5;
        KillUnits(result.InjuredParty1, party1, SURVIVE_CHANCE);
        KillUnits(result.InjuredParty2, party2, SURVIVE_CHANCE);
    }

    private static void ApplyDamage(List<UnitInstance> simulatedUnits, PartyController party)
    {
        foreach (var simulated in simulatedUnits)
        {
            var actual = party.PartyMembers.Find(u => u.ID == simulated.ID);
            if (actual != null)
                actual.SetHealth(simulated.Health);
        }
    }

    private static bool DecideDeath(double surviveChance)
    {
        System.Random rng = new System.Random();
        return rng.NextDouble() > surviveChance;
    }


    private static void KillUnits(List<UnitInstance> injuredUnits, PartyController party, double surviveChance)
    {
        foreach (var injured in injuredUnits)
        {
            var actual = party.PartyMembers.Find(u => u.ID == injured.ID);
            if (actual == null) Debug.LogWarning("Unit not found. Attempting to kill unit");

            // unit dies
            if (DecideDeath(surviveChance))
            {
                if (actual != null && actual.Health <= 0)
                    party.RemoveUnit(actual);
            }
            else
            {
                actual.Heal(1); // give 1 point of health
            }
        }
    }
}