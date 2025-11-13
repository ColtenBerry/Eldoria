using UnityEngine;
using UnityEngine.UI;

public class WeeklyEarningsPanelController : MonoBehaviour
{
    [SerializeField] private GameObject earningsItemPrefab;
    [SerializeField] private Transform earningsItemsParent; // e.g. a VerticalLayoutGroup container
    [SerializeField] private Transform totalsParent;

    [SerializeField] private Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseWeeklyEarningsMenu();
        });
    }

    public void OpenMenu(EarningData[] weeklyEarnings)
    {
        gameObject.SetActive(true);

        // Clear old items
        foreach (Transform child in earningsItemsParent)
        {
            Destroy(child.gameObject);
        }

        // Populate new items
        foreach (var earning in weeklyEarnings)
        {
            var itemGO = Instantiate(earningsItemPrefab, earningsItemsParent);
            var itemController = itemGO.GetComponent<EarningsItemController>();
            itemController.SetData(earning.Description, earning.Amount);
        }
        int totalEarnings = 0;
        foreach (var earning in weeklyEarnings)
        {
            totalEarnings += earning.Amount;
        }
        GameManager.Instance.PlayerProfile.AddGold(totalEarnings);
        SetTotals(totalEarnings);
    }

    private void SetTotals(int totalEarnings)
    {
        // Clear old totals
        foreach (Transform child in totalsParent)
        {
            Destroy(child.gameObject);
        }

        // Create new totals item
        var totalsGO = Instantiate(earningsItemPrefab, totalsParent);
        var totalsController = totalsGO.GetComponent<EarningsItemController>();
        totalsController.SetData("Total Earnings", totalEarnings);

        // Create new total balance item
        var balanceGO = Instantiate(earningsItemPrefab, totalsParent);
        var balanceController = balanceGO.GetComponent<EarningsItemController>();
        balanceController.SetData("Total Balance", GameManager.Instance.PlayerProfile.GoldAmount);
    }

}

// Example data struct
[System.Serializable]
public struct EarningData
{
    public string Description;
    public int Amount;

    public EarningData(string description, int amount)
    {
        Description = description;
        Amount = amount;
    }
}
