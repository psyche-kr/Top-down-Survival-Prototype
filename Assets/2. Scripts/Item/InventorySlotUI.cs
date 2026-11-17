using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    ItemSlotUI view;
    Button button;

    void Awake()
    {
        view = GetComponent<ItemSlotUI>();
        button = GetComponent<Button>();
    }

    public void Init(ItemStack stack)
    {
        view?.SetData(stack);

        button?.onClick.RemoveAllListeners();
        button?.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        Debug.Log("인벤토리 아이템 클릭됨");
        // 장착, 클릭, 드래그, 사용 등
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var stack = view.GetStack();

        if (stack == null || stack.IsEmpty) return;

        ItemTooltipUI.Instance.Show(stack.item, TooltipOwner.Inventory);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.Instance.Hide();
    }
}