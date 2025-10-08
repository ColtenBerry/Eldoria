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

    private ICardHandler<ItemStack> controller;


    public void Setup(ItemStack stack, ICardHandler<ItemStack> handler)
    {
        this.stack = stack;
        //icon.sprite = stack.item.icon;
        quantityText.text = stack.quantity >= 1 ? stack.quantity.ToString() : "";
        itemNameText.text = stack.item.itemName;
        controller = handler;

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
        controller.OnCardClicked(stack);
        Select();
    }
    public ItemStack GetStack()
    {
        return stack;
    }
}
