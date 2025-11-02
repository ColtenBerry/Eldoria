using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLordProfile", menuName = "Game/Lord Profile")]
public class LordProfileSO : ScriptableObject
{
    [Header("Lord Identity")]
    public CharacterData lordData;

    [Header("Faction")]
    public Faction faction;

    [Header("Owned Territories")]
    public List<Settlement> ownedTerritories;

    [Header("Starting Units")]
    public List<UnitData> startingUnits;
}
