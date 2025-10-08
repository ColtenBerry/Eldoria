using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour, ICardHandler<ItemStack>
{
    public Transform itemGridParent;
    public ItemSlotUI itemSlotPrefab;
    private ItemStack currentSelected;
    [SerializeField]
    private Button dropStackButton;

    [SerializeField] private InventoryManager inventory;

    private void Awake()
    {
        dropStackButton.onClick.AddListener(() =>
        {
            if (currentSelected == null) return;
            Debug.Log("Drop Stack button clicked");
            inventory.RemoveItem(currentSelected.item, currentSelected.quantity);
            RefreshUI(inventory.Inventory.GetAllItems());
        });

        RefreshUI(inventory.Inventory.GetAllItems());
    }

    public void OnEnable()
    {
        RefreshUI(inventory.Inventory.GetAllItems());
    }

    public void OnDisable()
    {
        currentSelected = null;
        Debug.Log("current selected is now null");
    }

    public void RefreshUI(List<ItemStack> stacks)
    {
        Debug.Log("Refreshing UI");
        foreach (Transform child in itemGridParent)
            Destroy(child.gameObject);

        foreach (var stack in stacks)
        {
            var slot = Instantiate(itemSlotPrefab, itemGridParent);
            if (currentSelected != null && stack.item == currentSelected.item && stack.quantity == currentSelected.quantity) slot.Select();
            slot.GetComponent<ItemSlotUI>().Setup(stack, this);
        }
    }


    public void OnCardClicked(ItemStack data)
    {
        currentSelected = data;
        RefreshUI(inventory.Inventory.GetAllItems());
    }
}

