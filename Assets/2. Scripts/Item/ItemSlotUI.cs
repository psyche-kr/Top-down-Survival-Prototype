using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Text amountText;

    ItemStack currentStack;

    public void SetData(ItemStack stack)
    {
        currentStack = stack;

        if (stack == null || stack.IsEmpty)
        {
            iconImage.enabled = false;
            amountText.gameObject.SetActive(false);
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = stack.item.icon;

        if (stack.amount > 1)
        {
            amountText.gameObject.SetActive(true);
            amountText.text = stack.amount.ToString();
        }
        else
        {
            amountText.gameObject.SetActive(false);
        }
    }

    public ItemStack GetStack() => currentStack;
}