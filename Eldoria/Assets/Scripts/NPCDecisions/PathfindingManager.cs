using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;


public class PathNode
{
    public Vector3Int Position;
    public float GCost; // Cost from start
    public float HCost; // Heuristic to end
    public float FCost => GCost + HCost;
    public PathNode Parent;

    public PathNode(Vector3Int position)
    {
        Position = position;
        GCost = float.MaxValue;
    }
}

public class PathNodeComparer : IComparer<PathNode>
{
    public int Compare(PathNode a, PathNode b)
    {
        int fCompare = a.FCost.CompareTo(b.FCost);
        if (fCompare != 0) return fCompare;

        // Tie-breaker to avoid collisions
        return a.Position.GetHashCode().CompareTo(b.Position.GetHashCode());
    }
}

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance;

    [SerializeField] private Tilemap terrainMap;
    [SerializeField] private Tilemap obstacleMap;

    private void Awake()
    {
        Instance = this;
    }

    public List<Vector3> FindPath(Vector3 startWorld, Vector3 endWorld)
    {
        Vector3Int start = terrainMap.WorldToCell(startWorld);
        Vector3Int end = terrainMap.WorldToCell(endWorld);

        var openSet = new SortedSet<PathNode>(new PathNodeComparer());
        var closedSet = new HashSet<Vector3Int>();
        var allNodes = new Dictionary<Vector3Int, PathNode>();

        PathNode startNode = new(start) { GCost = 0, HCost = Heuristic(start, end) };
        openSet.Add(startNode);
        allNodes[start] = startNode;

        var directions = new Vector3Int[]
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right,
            new Vector3Int(1, 1, 0),    // up-right
            new Vector3Int(-1, 1, 0),   // up-left
            new Vector3Int(1, -1, 0),   // down-right
            new Vector3Int(-1, -1, 0)   // down-left
        };

        while (openSet.Count > 0)
        {
            PathNode current = openSet.Min;
            openSet.Remove(current);

            if (closedSet.Contains(current.Position)) continue;
            closedSet.Add(current.Position);

            if (current.Position == end)
                return ReconstructPath(current);

            foreach (var dir in directions)
            {
                Vector3Int neighborPos = current.Position + dir;

                if (closedSet.Contains(neighborPos)) continue;
                if (!terrainMap.HasTile(neighborPos)) continue;
                if (obstacleMap.HasTile(neighborPos)) continue;

                bool isDiagonal = Mathf.Abs(dir.x) + Mathf.Abs(dir.y) == 2;

                // Optional: prevent diagonal corner clipping
                if (isDiagonal)
                {
                    Vector3Int sideA = new Vector3Int(current.Position.x + dir.x, current.Position.y, 0);
                    Vector3Int sideB = new Vector3Int(current.Position.x, current.Position.y + dir.y, 0);
                    if (!terrainMap.HasTile(sideA) || !terrainMap.HasTile(sideB)) continue;
                }

                float baseCost = GetMoveCost(neighborPos);
                if (baseCost < 0f) continue;

                float moveCost = baseCost * (isDiagonal ? 1.4142f : 1f);
                float tentativeG = current.GCost + moveCost;

                if (!allNodes.TryGetValue(neighborPos, out var neighbor))
                {
                    neighbor = new PathNode(neighborPos);
                    allNodes[neighborPos] = neighbor;
                }

                if (tentativeG < neighbor.GCost)
                {
                    if (openSet.Contains(neighbor))
                        openSet.Remove(neighbor);

                    neighbor.GCost = tentativeG;
                    neighbor.HCost = Heuristic(neighborPos, end);
                    neighbor.Parent = current;
                    openSet.Add(neighbor);
                }
            }
        }

        return null; // no path found
    }

    private float GetMoveCost(Vector3Int cell)
    {
        Vector3 worldPos = terrainMap.GetCellCenterWorld(cell);
        float speed = MovementCostManager.Instance.GetSpeedMultiplier(worldPos);
        return speed > 0f ? 1f / Mathf.Clamp(speed, 0.1f, 10f) : -1f;
    }

    private float Heuristic(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) + (1.4142f - 2f) * Mathf.Min(dx, dy); // Octile distance
    }

    private List<Vector3> ReconstructPath(PathNode endNode)
    {
        var path = new List<Vector3>();
        PathNode current = endNode;

        while (current != null)
        {
            path.Add(terrainMap.GetCellCenterWorld(current.Position));
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }
}