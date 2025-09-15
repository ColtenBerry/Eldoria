using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Food")]
public class FoodItem : InventoryItem
{
    public int restoreAmount;
    public bool affectsMorale;
}
