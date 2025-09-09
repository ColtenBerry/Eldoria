using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField]
    private Image icon;
    [SerializeField]
    private TextMeshProUGUI quantityText;

    private ItemStack stack;

    public void Setup(ItemStack stack)
    {
        this.stack = stack;
        //icon.sprite = stack.item.icon;
        quantityText.text = stack.quantity >= 1 ? stack.quantity.ToString() : "";
    }

}
