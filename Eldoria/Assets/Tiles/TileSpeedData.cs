using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(
    fileName = "TileSpeedData",
    menuName = "Movement/Single Tile Speed",
    order = 100)]
public class TileSpeedData : ScriptableObject
{
    public TileBase tile;
    public float speedMultiplier = 1f; //Default is "normal" speed
}
