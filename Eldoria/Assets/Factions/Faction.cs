using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Faction")]
public class Faction : ScriptableObject
{
    public string factionName;
    public Sprite icon;
    public Color factionColor;

}