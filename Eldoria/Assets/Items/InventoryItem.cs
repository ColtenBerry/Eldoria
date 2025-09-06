using UnityEngine;


public enum ItemCategory
{
    Weapon,
    Armour,
    Food,
    Misc
}
public abstract class InventoryItem : ScriptableObject
{
    public string itemName;
    //public Sprite icon;
    public ItemCategory category;
    public int value;
    public string description;
    public bool isStackable;
    public int maxStackSize = 60;
}

[CreateAssetMenu(menuName = "Inventory/Weapon")]
public class WeaponItem : InventoryItem
{
    public int damage;
}

[CreateAssetMenu(menuName = "Inventory/Armour")]
public class ArmourItem : InventoryItem
{
    public int defense;
}

[CreateAssetMenu(menuName = "Inventory/Food")]
public class FoodItem : InventoryItem
{
    public int restoreAmount;
    public bool affectsMorale;
}
