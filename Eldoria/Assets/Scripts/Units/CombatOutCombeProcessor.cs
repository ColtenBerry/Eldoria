using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public static class CombatOutcomeProcessor
{
    public static void ApplyCombatResult(CombatResult result, List<PartyController> attackers, List<PartyController> defenders)
    {
        foreach (var party in attackers)
        {
            var relevantSimulated = result.AttackerUnits
                .Where(u => party.PartyMembers.Any(p => p.ID == u.ID))
                .ToList();

            ApplyDamage(relevantSimulated, party, 0.5);
        }

        foreach (var party in defenders)
        {
            var relevantSimulated = result.DefenderUnits
                .Where(u => party.PartyMembers.Any(p => p.ID == u.ID))
                .ToList();

            ApplyDamage(relevantSimulated, party, 0.5);
        }

        var defeatedDefenders = result.DefenderUnits.Where(u => u.Health <= 0).ToList();
        foreach (var party in attackers)
            RewardExperience(result.AttackerUnits, party, defeatedDefenders);

        var defeatedAttackers = result.AttackerUnits.Where(u => u.Health <= 0).ToList();
        foreach (var party in defenders)
            RewardExperience(result.DefenderUnits, party, defeatedAttackers);

    }

    private static List<UnitInstance> GetDefeatedUnits(List<UnitInstance> simulatedUnits)
    {
        return simulatedUnits.Where(u => u.Health <= 0).ToList();
    }


    public static List<UnitInstance> ReturnPrisoners(List<PartyController> losingParties)
    {
        return losingParties
                .SelectMany(party => party.PartyMembers)
                .ToList();
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


    public static void ProcessAutoResolveResult(CombatResult result, List<PartyController> attackers, List<PartyController> defenders, bool isSiegeBattle, Settlement fief)
    {
        ApplyCombatResult(result, attackers, defenders);

        bool attackersWin = result.AttackersWin;

        if (attackersWin)
        {
            if (isSiegeBattle)
            {
                TerritoryManager.Instance.RegisterOwnership(fief, attackers.First().GetComponent<PartyPresence>().Lord);

                // Wipe garrison units but keep GameObject
                var garrison = defenders.FirstOrDefault();
                if (garrison != null)
                    garrison.PartyMembers.Clear();

                // Destroy other defenders
                for (int i = 1; i < defenders.Count; i++)
                    Object.Destroy(defenders[i].gameObject);
            }
            else
            {
                foreach (var party in defenders)
                    Object.Destroy(party.gameObject);
            }
        }
        else
        {
            if (isSiegeBattle)
            {
                TerritoryManager.Instance.RegisterOwnership(fief, defenders.First().GetComponent<PartyPresence>().Lord);

                foreach (var party in attackers)
                    Object.Destroy(party.gameObject);
            }
            else
            {
                foreach (var party in attackers)
                    Object.Destroy(party.gameObject);
            }
        }
    }

    public static void ProcessPlayerBattleResult(CombatResult result, List<PartyController> attackers, List<PartyController> defenders, bool isPlayerAttacking, bool isSiegeBattle, Settlement fief)
    {
        ApplyCombatResult(result, attackers, defenders);

        bool playerWon = (result.AttackersWin && isPlayerAttacking) || (!result.AttackersWin && !isPlayerAttacking);


        if (playerWon)
        {
            var losers = isPlayerAttacking ? defenders : attackers;
            var prisoners = ReturnPrisoners(losers);
            UIManager.Instance.OpenSubMenu("prisoners", prisoners);

            if (isSiegeBattle)
            {
                if (isPlayerAttacking)
                {
                    TerritoryManager.Instance.RegisterOwnership(fief, GameManager.Instance.PlayerProfile);

                    // Wipe garrison units but keep GameObject
                    if (defenders.Count > 0)
                        defenders[0].PartyMembers.Clear();

                    for (int i = 1; i < defenders.Count; i++)
                        Object.Destroy(defenders[i].gameObject);
                }
                else
                {
                    foreach (var party in attackers)
                    {
                        // TODO: if party is player, handle differently
                        Object.Destroy(party.gameObject);
                    }
                }
            }
            else
            {
                foreach (var party in losers)
                {
                    // TODO: if party is player, handle differently
                    Object.Destroy(party.gameObject);
                }
            }
        }
        else // player lost
        {
            if (isSiegeBattle)
            {
                var attackerPresence = attackers.FirstOrDefault()?.GetComponent<PartyPresence>();
                if (attackerPresence == null)
                {
                    Debug.LogWarning("attacker party presence is null");
                    return;
                }

                var newOwner = LordRegistry.Instance.GetLordByName(attackerPresence.Lord.Lord.UnitName);
                TerritoryManager.Instance.RegisterOwnership(fief, newOwner);
            }

            var losers = isPlayerAttacking ? attackers : defenders;
            foreach (var party in losers)
            {
                // TODO: if party is player, handle differently
                Object.Destroy(party.gameObject);
            }
        }
    }


}