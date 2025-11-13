using TMPro;
using UnityEngine;

public class EarningsItemController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI amountText;

    public void SetData(string description, int amount)
    {
        descriptionText.text = description;
        if (amount > 0)
        {
            amountText.color = Color.green;
        }
        else if (amount < 0)
        {
            amountText.color = Color.red;
        }
        else
        {
            amountText.color = Color.white;
        }
        amountText.text = amount.ToString();
    }
}
