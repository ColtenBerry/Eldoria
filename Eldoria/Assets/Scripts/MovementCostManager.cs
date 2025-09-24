using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementCostManager : MonoBehaviour
{
    public static MovementCostManager Instance { get; private set; }

    [Header("Tilemap References")]
    [SerializeField] private Tilemap terrainTilemap;
    [SerializeField] private Tilemap obstacleTilemap;

    [Header("Speed Data Assets")]
    [SerializeField] private List<TileSpeedData> terrainSpeedDatas;
    [SerializeField] private List<TileSpeedData> obstacleSpeedDatas;

    private Dictionary<TileBase, float> terrainDict;
    private Dictionary<TileBase, float> obstacleDict;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        BuildDictionary(terrainSpeedDatas, out terrainDict);
        BuildDictionary(obstacleSpeedDatas, out obstacleDict);
    }

    private void BuildDictionary(
        List<TileSpeedData> dataList,
        out Dictionary<TileBase, float> dict)
    {
        dict = new Dictionary<TileBase, float>();
        foreach (var data in dataList)
            if (data != null && data.tile != null)
                dict[data.tile] = data.speedMultiplier;
    }

    public float GetSpeedMultiplier(Vector3 worldPos)
    {
        Vector3Int cell = terrainTilemap.WorldToCell(worldPos);

        float terrainMul = 1f;
        terrainDict.TryGetValue(terrainTilemap.GetTile(cell), out terrainMul);

        float obstacleMul = 1f;

        if (obstacleTilemap.GetTile(cell) == null) return terrainMul;

        obstacleDict.TryGetValue(obstacleTilemap.GetTile(cell), out obstacleMul);

        return terrainMul * obstacleMul;
    }
}
