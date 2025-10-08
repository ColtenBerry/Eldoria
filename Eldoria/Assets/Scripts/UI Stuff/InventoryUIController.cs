using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    public Transform itemGridParent;
    public ItemSlotUI itemSlotPrefab;
    private ItemSlotUI currentSelected;
    [SerializeField]
    private Button dropStackButton;

    [SerializeField] private PlayerInventoryManager inventory;

    private void Awake()
    {
        dropStackButton.onClick.AddListener(() =>
        {
            if (currentSelected == null) return;
            Debug.Log("Drop Stack button clicked");
            inventory.RemoveItem(currentSelected.GetStack().item, currentSelected.GetStack().quantity);
            RefreshUI(inventory.Inventory.GetAllItems());
        });

        RefreshUI(inventory.Inventory.GetAllItems());
    }

    public void OnDisable()
    {
        if (currentSelected != null) currentSelected.Deselect();
        currentSelected = null;
    }

    public void RefreshUI(List<ItemStack> stacks)
    {
        Debug.Log("Refreshing UI");
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

