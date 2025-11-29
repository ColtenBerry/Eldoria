using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLordProfile", menuName = "Game/Lord Profile")]
public class LordProfileSO : ScriptableObject
{
    [Header("Lord Identity")]
    public CharacterData lordData;

    [Header("Faction")]
    public Faction faction;

    [Header("Starting Units")]
    public SoldierGroup startingSoldiers;

    [Header("Economy")]
    public int startingGold = 5000;
}
