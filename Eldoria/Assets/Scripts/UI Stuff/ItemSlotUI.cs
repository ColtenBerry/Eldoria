using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image icon;
    [SerializeField]
    private TextMeshProUGUI quantityText;
    [SerializeField]
    private TextMeshProUGUI itemNameText;

    private ItemStack stack;
    [SerializeField]
    //private Image highlightImage; // maybe used later. 

    private InventoryUIController controller;

    public void Awake()
    {
        controller = transform.parent.parent.GetComponent<InventoryUIController>();
    }

    public void Setup(ItemStack stack)
    {
        this.stack = stack;
        //icon.sprite = stack.item.icon;
        quantityText.text = stack.quantity >= 1 ? stack.quantity.ToString() : "";
        itemNameText.text = stack.item.itemName;

    }
    public void Select()
    {
        //highlightImage.enabled = true;
        gameObject.GetComponent<Image>().color = Color.yellow;
    }
    public void Deselect()
    {
        //highlightImage.enabled = false;
        gameObject.GetComponent<Image>().color = Color.white;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        controller.SelectSlot(this);
    }
}
