using System.Collections.Generic;
using UnityEngine;


// for creating starting groups of soldiers
[CreateAssetMenu(fileName = "SoldierGroup", menuName = "Scriptable Objects/SoldierGroup")]
public class SoldierGroup : ScriptableObject
{
    public List<SoldierData> soldiers = new();
}
