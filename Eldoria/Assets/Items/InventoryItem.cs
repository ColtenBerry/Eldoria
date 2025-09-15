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
    public Sprite icon;
    public ItemCategory category;
    public int value;
    public string description;
    public bool isStackable;
    public int maxStackSize = 60;
}
