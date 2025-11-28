using System.Collections.Generic;
using UnityEngine;

public class HungerManager : MonoBehaviour
{
    private PartyPresence partyPresence;
    private InventoryManager inventoryManager;

    private void Awake()
    {
        partyPresence = GetComponent<PartyPresence>();
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
        int foodConsumption = partyPresence.PartyController.CalculateFoodConsumption();

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
        partyPresence.PartyController.SetIsStarving(foodConsumption > 0);
        if (foodConsumption > 0)
        {
            partyPresence.PartyController.HandleStarvation(foodConsumption);
        }
    }
}