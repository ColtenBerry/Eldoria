using System.Collections.Generic;
using System.Linq;
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting;
using UnityEngine;

public static class CombatOutcomeProcessor
{
    public static void ApplyCombatResult(CombatResult result, List<PartyController> attackers, List<PartyController> defenders, List<CharacterInstance> attackingLords, List<CharacterInstance> defendingLords)
    {
        foreach (var party in attackers)
        {
            var relevantSimulated = result.AttackerUnits
                .Where(u => party.PartyMembers.Any(p => p.ID == u.ID))
                .ToList();

            ApplyPartyDamage(relevantSimulated, party, 0.5);
        }

        var relevantAttackingLords = result.AttackerUnits
            .Where(u => attackingLords.Any(l => l.ID == u.ID))
            .ToList();
        ApplyLordDamage(relevantAttackingLords, attackingLords, 0.5);

        foreach (var party in defenders)
        {
            var relevantSimulated = result.DefenderUnits
                .Where(u => party.PartyMembers.Any(p => p.ID == u.ID))
                .ToList();

            ApplyPartyDamage(relevantSimulated, party, 0.5);
        }

        var relevantDefendingLords = result.DefenderUnits
            .Where(u => defendingLords.Any(l => l.ID == u.ID))
            .ToList();
        ApplyLordDamage(relevantDefendingLords, defendingLords, 0.5);


        var defeatedDefenders = result.DefenderUnits.Where(u => u.Health <= 0).ToList();
        foreach (var party in attackers)
            RewardExperience(result.AttackerUnits, party, defeatedDefenders);

        var defeatedAttackers = result.AttackerUnits.Where(u => u.Health <= 0).ToList();
        foreach (var party in defenders)
            RewardExperience(result.DefenderUnits, party, defeatedAttackers);

    }

    private static List<SoldierInstance> GetDefeatedUnits(List<SoldierInstance> simulatedUnits)
    {
        return simulatedUnits.Where(u => u.Health <= 0).ToList();
    }


    public static List<SoldierInstance> ReturnPrisoners(List<PartyController> losingParties)
    {
        return losingParties
                .SelectMany(party => party.PartyMembers)
                .ToList();
    }


    private static void ApplyPartyDamage(List<UnitInstance> simulatedUnits, PartyController party, double surviveChance)
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
            ApplyUnitDamage(simulated, actual, toRemove, surviveChance);

        }

        foreach (UnitInstance unit in toRemove)
        {
            if (unit is SoldierInstance soldier)
            {
                party.RemoveUnit(soldier);
            }
            else if (unit is CharacterInstance character)
            {
                Debug.LogWarning("Something isn't working right. this should be for partymembers, not characters");
                // decide fate here. 
            }
        }
    }

    private static void ApplyLordDamage(List<UnitInstance> simulatedUnits, List<CharacterInstance> lords, double surviveChance)
    {
        List<UnitInstance> toRemove = new();

        foreach (var simulated in simulatedUnits)
        {
            var actual = lords.Find(l => l.ID == simulated.ID);
            if (actual == null)
            {
                Debug.LogWarning($"Simulated lord ID {simulated.ID} not found among lords.");
                continue;
            }

            ApplyUnitDamage(simulated, actual, toRemove, surviveChance);
        }

        foreach (UnitInstance unit in toRemove)
        {
            if (unit is CharacterInstance character)
            {
                // Decide fate here — maybe mark as dead, captured, or trigger a narrative event
                bool b = DecideDeath(.5);
                if (b)
                {
                    // character "dies". notify respawn
                    LordProfile p = LordRegistry.Instance.GetLordByName(character.UnitName);
                    FactionWarManager warManager = FactionsManager.Instance.GetWarManager(p.Faction);
                    warManager.AddToPendingRespawns(p);

                    lords.Remove(character);
                }
                else
                {
                    // add to prisoner list? 
                    // do nothing since we are not removing from the lords list
                }
            }
            else
            {
                Debug.LogWarning("something not working right. should be for lords");
            }
        }
    }



    private static void ApplyUnitDamage(UnitInstance simulated, UnitInstance actual, List<UnitInstance> toRemove, double surviveChance)
    {
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


    public static void ProcessAutoResolveResult(CombatResult result, List<PartyController> attackers, List<PartyController> defenders, List<CharacterInstance> attackingLords, List<CharacterInstance> defendingLords, bool isSiegeBattle, Settlement fief)
    {
        ApplyCombatResult(result, attackers, defenders, attackingLords, defendingLords);

        bool attackersWin = result.AttackersWin;

        List<PartyController> losers = result.AttackersWin ? defenders : attackers;
        List<PartyController> winners = result.AttackersWin ? attackers : defenders;


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
                {
                    NotifyAndDestroyParty(defenders[i]);
                }
            }
            else
            {
                foreach (var party in defenders)
                    NotifyAndDestroyParty(party);
            }
        }
        else
        {
            if (isSiegeBattle)
            {
                // TerritoryManager.Instance.RegisterOwnership(fief, defenders.First().GetComponent<PartyPresence>().Lord);

                foreach (var party in attackers)
                    NotifyAndDestroyParty(party);
            }
            else
            {
                foreach (var party in attackers)
                    NotifyAndDestroyParty(party);
            }
        }

        // print the message
        LogMessage(losers.First().gameObject.name, winners.First().gameObject.name);

    }

    public static void ProcessPlayerBattleResult(CombatResult result, List<PartyController> attackers, List<PartyController> defenders, List<CharacterInstance> attackingLords, List<CharacterInstance> defendingLords, bool isPlayerAttacking, bool isSiegeBattle, Settlement fief)
    {
        List<PartyController> losers = result.AttackersWin ? defenders : attackers;
        List<PartyController> winners = result.AttackersWin ? attackers : defenders;
        List<CharacterInstance> losingLords = result.AttackersWin ? defendingLords : attackingLords;

        List<ItemStack> potentialLoot = GenerateLootFromPartyList(losers);
        int goldEarned = CalculateValueFromPartyList(losers);

        ApplyCombatResult(result, attackers, defenders, attackingLords, defendingLords);

        bool playerWon = (result.AttackersWin && isPlayerAttacking) || (!result.AttackersWin && !isPlayerAttacking);

        if (playerWon)
        {
            List<UnitInstance> prisoners = new();

            prisoners.AddRange(ReturnPrisoners(losers));
            prisoners.AddRange(losingLords);
            PrisonerAndLootMenuContext ctx = new PrisonerAndLootMenuContext(prisoners, goldEarned, potentialLoot);

            UIManager.Instance.OpenSubMenu("prisoners", ctx);

            if (isSiegeBattle)
            {
                if (isPlayerAttacking)
                {
                    TerritoryManager.Instance.RegisterOwnership(fief, GameManager.Instance.PlayerProfile);

                    // Wipe garrison units but keep GameObject
                    if (defenders.Count > 0)
                        defenders[0].PartyMembers.Clear();

                    for (int i = 1; i < defenders.Count; i++)
                        NotifyAndDestroyParty(defenders[i]);
                }
                else
                {
                    foreach (var party in attackers)
                    {
                        // TODO: if party is player, handle differently
                        NotifyAndDestroyParty(party);
                    }
                }
            }
            else
            {
                foreach (var party in losers)
                {
                    // TODO: if party is player, handle differently
                    NotifyAndDestroyParty(party);
                }
            }

            // throws an error because the garrison does not have a lord. 
            // UIManager.Instance.LogMessage(new WorldMessage($"{losers.First().GetComponent<PartyPresence>().Lord.Lord.UnitName} has been defeated in battle."));

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

            losers = isPlayerAttacking ? attackers : defenders;
            foreach (var party in losers)
            {
                // TODO: if party is player, handle differently
                NotifyAndDestroyParty(party);
            }
        }
        LogMessage(losers.First().gameObject.name, winners.First().gameObject.name);

    }

    private static void NotifyAndDestroyParty(PartyController party)
    {

        PartyPresence presence = party.GetComponent<PartyPresence>();
        if (presence == null)
        {
            Debug.LogWarning("PartyPresence component not found on party to be destroyed.");
            return;
        }
        LordProfile lord = presence.Lord;
        FactionWarManager factionWarManager;
        factionWarManager = FactionsManager.Instance.GetWarManager(lord.Faction);
        if (factionWarManager != null)
        {
            factionWarManager.NotifyPartyDestroyed(lord);

        }

        if (party.gameObject != GameManager.Instance.player)
        {
            GameObject.Destroy(party.gameObject);
        }

        else
        {
            Debug.Log("PlayerDeath");
            // handle player death
            if (TerritoryManager.Instance.GetSettlementsOf(GameManager.Instance.PlayerProfile).Count > 0)
                party.gameObject.transform.position = TerritoryManager.Instance.GetSettlementsOf(GameManager.Instance.PlayerProfile).First().gameObject.transform.position;
            else
            {
                party.gameObject.transform.position = new Vector3(0, 0);
            }
        }
    }

    public static void LogMessage(string loserName, string winnerName)
    {
        UIManager.Instance.LogMessage(new WorldMessage($"{loserName} has been defeated by {winnerName} in battle."));
    }

    #region LootRegion
    private static System.Random rng = new System.Random();

    /// <summary>
    /// Calculates the value of a defeated unit based on its stats.
    /// Divisor controls overall scaling; higher means less value.
    /// </summary>
    /// <param name="enemy"></param>
    /// <param name="DIVISOR"></param>
    /// <returns></returns>
    private static int CalculateValueFromUnit(SoldierInstance enemy, int DIVISOR = 2)
    {
        int statSum = enemy.Attack + enemy.Defence + enemy.Health + enemy.Moral;
        Debug.Log("health: " + enemy.Health);
        Debug.Log("stat sum: " + statSum);
        int baseGold = Mathf.Max(1, statSum / DIVISOR);

        // Add ±20% variance
        float VARIANCE = 0.2f;
        float multiplier = 1f + ((float)rng.NextDouble() * 2f - 1f) * VARIANCE;
        int randomizedGold = Mathf.RoundToInt(baseGold * multiplier);

        return Mathf.Max(1, randomizedGold);
    }

    private static int CalculateValueFromPartyList(List<PartyController> parties, int DIVISOR = 2)
    {
        int totalLootValue = 0;
        foreach (var party in parties)
        {
            foreach (var unit in party.PartyMembers)
            {
                totalLootValue += CalculateValueFromUnit(unit, DIVISOR);
            }
        }
        return totalLootValue;
    }

    //TODO: Expand to include items, etc.

    private static List<ItemStack> GenerateLootFromPartyList(List<PartyController> parties)
    {
        List<ItemStack> loot = new();
        int value = CalculateValueFromPartyList(parties);

        while (value > 0)
        {
            InventoryItem itemData = ItemDatabase.GetRandomItem();
            int itemValue = itemData.baseCost;

            if (itemValue <= value)
            {
                // Check if this item already exists in the loot list
                var existingStack = loot.FirstOrDefault(stack => stack.item == itemData);
                if (existingStack != null)
                {
                    existingStack.quantity += 1;
                }
                else
                {
                    loot.Add(new ItemStack(itemData, 1));
                }

                value -= itemValue;
            }
            else
            {
                break;
            }
        }

        // Placeholder: Currently no items are generated
        return loot;
    }


    #endregion

}