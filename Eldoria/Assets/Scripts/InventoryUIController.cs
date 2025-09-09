using System.Collections.Generic;
using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    public Transform itemGridParent;
    public GameObject itemSlotPrefab;

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
}

