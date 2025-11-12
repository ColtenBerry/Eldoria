using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    [SerializeField] private List<InventoryItem> items = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persist across scenes
    }

    public static InventoryItem GetRandomItem(int maxValue = int.MaxValue)
    {
        if (Instance == null || Instance.items == null || Instance.items.Count == 0)
            return null;

        var candidates = Instance.items.Where(i => i.baseCost <= maxValue).ToList();
        if (candidates.Count == 0) return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    public static InventoryItem GetByName(string name)
    {
        return Instance?.items.FirstOrDefault(i => i.name == name);
    }

    public static List<InventoryItem> GetAll()
    {
        return Instance?.items ?? new List<InventoryItem>();
    }
}
