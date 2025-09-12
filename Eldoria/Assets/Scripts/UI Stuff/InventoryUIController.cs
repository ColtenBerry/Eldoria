using System.Collections.Generic;
using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    public Transform itemGridParent;
    public ItemSlotUI itemSlotPrefab;
    private ItemSlotUI currentSelected;

    public void OnDisable()
    {
        if (currentSelected != null) currentSelected.Deselect();
        currentSelected = null;
    }

    public void RefreshUI(List<ItemStack> stacks)
    {
        foreach (Transform child in itemGridParent)
            Destroy(child.gameObject);

        foreach (var stack in stacks)
        {
            var slot = Instantiate(itemSlotPrefab, itemGridParent);
            slot.GetComponent<ItemSlotUI>().Setup(stack);
        }
    }

    public void SelectSlot(ItemSlotUI selected)
    {
        if (currentSelected != null) currentSelected.Deselect();
        currentSelected = selected;
        currentSelected.Select();
    }
}

