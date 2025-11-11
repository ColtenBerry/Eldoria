using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting;
using UnityEngine;

public static class CombatSimulator
{
    public static Action<PartyController, bool> OnCombatMenuRequested;
    public static void StartBattle(Vector3 location, Faction attackingFaction, Faction defendingFaction, bool isPlayerAttacking)
    {
        List<PartyPresence> nearbyPresences = GetNearbyParties(location, 30f);

        List<PartyController> attackers = nearbyPresences
    .Where(presence => presence.Lord.Faction == attackingFaction)
    .Select(presence => presence.GetComponent<PartyController>())
    .Where(controller => controller != null)
    .ToList();

        List<PartyController> defenders = nearbyPresences
            .Where(presence => presence.Lord.Faction == defendingFaction)
            .Select(presence => presence.GetComponent<PartyController>())
            .Where(controller => controller != null)
            .ToList();


        var enemyName = defenders.FirstOrDefault()?.name ?? "Unknown";
        CombatMenuContext ctx = new CombatMenuContext(
            friendlyParties: isPlayerAttacking ? attackers : defenders,
            enemyParties: isPlayerAttacking ? defenders : attackers,
            isPlayerAttacking: isPlayerAttacking,
            enemyName: enemyName


        );

        UIManager.Instance.OpenCombatMenu(ctx);
    }

    public static CombatResult StartBattle(Vector3 location, Faction attackingFaction, Faction defendingFaction)
    {
        List<PartyPresence> nearbyPresences = GetNearbyParties(location, 30f);

        List<PartyController> attackers = nearbyPresences
            .Where(p => p.Lord.Faction == attackingFaction)
            .Select(p => p.GetComponent<PartyController>())
            .Where(pc => pc != null)
            .ToList();

        List<PartyController> defenders = nearbyPresences
            .Where(p => p.Lord.Faction == defendingFaction)
            .Select(p => p.GetComponent<PartyController>())
            .Where(pc => pc != null)
            .ToList();

        CombatResult result = SimulateBattle(attackers, defenders);
        CombatOutcomeProcessor.ApplyCombatResult(result, attackers, defenders);
        CombatOutcomeProcessor.ProcessAutoResolveResult(result, attackers, defenders, false, null);
        return result;
    }

    public static CombatResult StartSiegeBattle(Vector3 location, Faction attackingFaction, Faction defendingFaction, Settlement targetFief)
    {
        List<PartyPresence> nearbyPresences = GetNearbyParties(location, 30f);

        List<PartyController> attackers = nearbyPresences
            .Where(p => p.Lord.Faction == attackingFaction)
            .Select(p => p.GetComponent<PartyController>())
            .Where(pc => pc != null)
            .ToList();

        List<PartyController> defenders = nearbyPresences
            .Where(p => p.Lord.Faction == defendingFaction)
            .Select(p => p.GetComponent<PartyController>())
            .Where(pc => pc != null)
            .ToList();
        PartyController garrison = targetFief.GetComponent<PartyController>();
        if (garrison != null)
        {
            defenders.Insert(0, garrison);
        }

        CombatResult result = SimulateBattle(attackers, defenders);
        CombatOutcomeProcessor.ApplyCombatResult(result, attackers, defenders);
        CombatOutcomeProcessor.ProcessAutoResolveResult(result, attackers, defenders, true, targetFief);
        return result;
    }



    public static void StartSiegeBattle(Vector3 location, Faction attackingFaction, Faction defendingFaction, Settlement targetFief, bool isPlayerAttacking)
    {
        List<PartyPresence> nearbyPresences = GetNearbyParties(location, 30f);

        List<PartyController> attackers = nearbyPresences
            .Where(p => p.Lord.Faction == attackingFaction)
            .Select(p => p.GetComponent<PartyController>())
            .Where(pc => pc != null)
            .ToList();

        List<PartyController> defenders = nearbyPresences
            .Where(p => p.Lord.Faction == defendingFaction)
            .Select(p => p.GetComponent<PartyController>())
            .Where(pc => pc != null)
            .ToList();

        var enemyName = defenders.FirstOrDefault()?.name ?? "Unknown";

        PartyController garrison = targetFief.GetComponent<PartyController>();
        if (garrison != null)
        {
            defenders.Insert(0, garrison);
        }


        CombatMenuContext ctx = new CombatMenuContext(
            friendlyParties: isPlayerAttacking ? attackers : defenders,
            enemyParties: isPlayerAttacking ? defenders : attackers,
            isPlayerAttacking: isPlayerAttacking,
            enemyName: enemyName,
            isSiegeBattle: true,
            fiefUnderSiege: targetFief
        );

        UIManager.Instance.OpenCombatMenu(ctx);
    }


    public static List<PartyPresence> GetNearbyParties(Vector3 battleLocation, float radius)
    {
        return UnityEngine.Object.FindObjectsOfType<PartyPresence>()
            .Where(p =>
                Vector3.Distance(p.transform.position, battleLocation) <= radius && p != GameManager.Instance.player.GetComponent<PartyPresence>())
            .ToList();
    }


    /// <summary>
    /// Simulates the battle itself
    /// </summary>
    /// <param name="party1"></param>
    /// <param name="party2"></param>
    /// <returns></returns>
    public static CombatResult SimulateBattle(List<PartyController> attackers, List<PartyController> defenders)
    {
        var result = new CombatResult
        {
            AttackerUnits = new List<UnitInstance>(),
            DefenderUnits = new List<UnitInstance>(),
            PartyUnitMap = new Dictionary<string, List<UnitInstance>>()
        };

        // Clone attacker units and track origin
        foreach (var party in attackers)
        {
            var clones = party.PartyMembers.Select(u => u.Clone()).ToList();
            result.AttackerUnits.AddRange(clones);
            result.PartyUnitMap[party.PartyID] = clones;
        }

        // Clone defender units and track origin
        foreach (var party in defenders)
        {
            var clones = party.PartyMembers.Select(u => u.Clone()).ToList();
            result.DefenderUnits.AddRange(clones);
            result.PartyUnitMap[party.PartyID] = clones;
        }

        int safetyCounter = 0;
        while (result.AttackerUnits.Count > 0 && result.DefenderUnits.Count > 0 && safetyCounter++ < 1000)
            SimulateRound(result);

        // After simulation loop
        result.AttackersWin = result.AttackerUnits.Count > 0;

        // Rehydrate full unit lists with updated stats
        var simulatedAttackers = result.AttackerUnits;
        var simulatedDefenders = result.DefenderUnits;

        result.AttackerUnits = attackers
    .SelectMany(party => party.PartyMembers)
    .Select(original =>
    {
        var simulated = simulatedAttackers.FirstOrDefault(sim => sim.ID == original.ID);
        if (simulated != null)
            return simulated;

        var clone = original.Clone();
        clone.SetHealth(0);
        return clone;
    })
    .ToList(); // ✅ This fixes the CS0266 error

        result.DefenderUnits = defenders
            .SelectMany(party => party.PartyMembers)
            .Select(original =>
            {
                var simulated = simulatedDefenders.FirstOrDefault(sim => sim.ID == original.ID);
                if (simulated != null)
                    return simulated;

                var clone = original.Clone();
                clone.SetHealth(0);
                return clone;
            })
            .ToList(); // ✅ Same fix here


        return result;
    }

    private static void SimulateRound(CombatResult result)
    {
        List<UnitInstance> attackers = new(result.AttackerUnits); // shallow copy for shuffle
        List<UnitInstance> defenders = new(result.DefenderUnits);
        ShuffleList(attackers);
        ShuffleList(defenders);
        Debug.Log("round called");

        // attackers attack
        for (int i = 0; i < attackers.Count; i++)
        {
            if (defenders.Count == 0) continue;
            int index = i % defenders.Count;
            SimulateAttack(attackers[i], defenders[index]);

        }
        result.AttackerUnits.RemoveAll(u => u.Health <= 0);
        result.DefenderUnits.RemoveAll(u => u.Health <= 0);


        // defenders attack
        for (int i = 0; i < defenders.Count; i++)
        {
            if (attackers.Count == 0) continue;
            int index = i % attackers.Count;
            SimulateAttack(defenders[i], attackers[index]);

        }

        result.AttackerUnits.RemoveAll(u => u.Health <= 0);
        result.DefenderUnits.RemoveAll(u => u.Health <= 0);


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
    public List<UnitInstance> AttackerUnits;
    public List<UnitInstance> DefenderUnits;
    public bool AttackersWin;

    public Dictionary<string, List<UnitInstance>> PartyUnitMap = new();
}