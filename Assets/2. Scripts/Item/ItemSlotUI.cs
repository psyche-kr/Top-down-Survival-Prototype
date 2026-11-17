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
        SetDimmed(false);

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

    /// <summary>드래그 중인 원래 칸의 아이콘을 흐리게 표시.</summary>
    public void SetDimmed(bool dimmed)
    {
        Color c = iconImage.color;
        c.a = dimmed ? 0.35f : 1f;
        iconImage.color = c;
    }
}
