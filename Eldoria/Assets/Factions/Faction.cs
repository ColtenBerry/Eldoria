using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Faction")]
public class Faction : ScriptableObject
{
    public string factionName;
    public Sprite icon;
    public Color factionColor;

    [Tooltip("Factions this one considers allies")]
    public List<Faction> initialAllies;

    [Tooltip("Factions this one considers enemies")]
    public List<Faction> initialEnemies;

    public bool IsAlliedWith(Faction other) => initialAllies.Contains(other);
    public bool IsEnemyOf(Faction other) => initialEnemies.Contains(other);
}