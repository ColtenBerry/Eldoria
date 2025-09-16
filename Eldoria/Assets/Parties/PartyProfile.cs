using System.Collections.Generic;
using UnityEngine;

public enum PartyType { Brigand, Lord, Caravan }

[CreateAssetMenu(fileName = "PartyProfile", menuName = "Scriptable Objects/PartyProfile")]
public class PartyProfile : ScriptableObject
{
    public PartyType type;
    public UnitData Lord;
    public string faction;
    public List<UnitData> startingUnits;
}
