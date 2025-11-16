using System.Collections.Generic;
using UnityEngine;

public class HungerManager : MonoBehaviour
{
    private PartyController partyController;
    private InventoryManager inventoryManager;

    private void Awake()
    {
        partyController = GetComponent<PartyController>();
        inventoryManager = GetComponent<InventoryManager>();
    }
    private bool subscribed = false;
    private void Update()
    {
        if (TickManager.Instance != null && !subscribed)
        {
            TickManager.Instance.OnDayPassed += HandleDailyFoodConsumption;
            subscribed = true;
        }
    }

    private void OnEnable()
    {
        if (TickManager.Instance != null)
        {
            TickManager.Instance.OnDayPassed += HandleDailyFoodConsumption;
            subscribed = true;
        }
    }

    private void OnDisable()
    {
        TickManager.Instance.OnDayPassed -= HandleDailyFoodConsumption;
    }

    private void HandleDailyFoodConsumption(int dayCount)
    {
        int foodConsumption = partyController.CalculateFoodConsumption();

        List<ItemStack> foodStacks = inventoryManager.GetAllItems().FindAll(stack => stack.item.category == ItemCategory.Food);

        foreach (ItemStack stack in foodStacks)
        {
            while (stack.quantity > 0 && foodConsumption > 0)
            {
                int amountToConsume = Mathf.Min(stack.quantity, foodConsumption);
                inventoryManager.RemoveItem(stack.item, amountToConsume);
                foodConsumption -= amountToConsume;
            }

            if (foodConsumption <= 0)
            {
                break;
            }
        }
        if (foodConsumption > 0)
        {
            partyController.HandleStarvation(foodConsumption);
        }
    }
}