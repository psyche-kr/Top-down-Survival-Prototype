using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 상자 한 칸. 왼클릭하면 인벤토리로 습득하고, 드래그해서 인벤토리 칸이나 바닥에 놓을 수 있다.
/// </summary>
public class ChestSlotUI :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    ItemSlotUI view;
    Chest currentChest;

    [SerializeField] Sprite filledBackground;
    [SerializeField] Sprite emptyBackground;

    Image backgroundImage;

    void Awake()
    {
        view = GetComponent<ItemSlotUI>();
        backgroundImage = GetComponent<Image>();
    }

    public void Init(ItemStack stack, Chest chest)
    {
        currentChest = chest;

        view.SetData(stack);

        backgroundImage.sprite =
            stack == null || stack.IsEmpty
            ? emptyBackground
            : filledBackground;
    }

    // 누른 칸과 드래그한 칸이 같으면 EventSystem은 드래그 뒤에도 클릭을 보낸다.
    // 그 클릭으로 습득하면 칸이 다시 만들어져 OnEndDrag가 오지 않으므로, 드래그였다면 클릭은 무시한다.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (eventData.dragging) return;

        ItemStack stack = view.GetStack();
        if (!currentChest.HasStack(stack)) return;

        // 인벤토리에 공간이 없으면 경고만 띄우고 상자에 남긴다
        if (!PlayerInventoryManager.Instance.AddItem(stack.item, stack.amount)) return;

        ItemTooltipUI.Instance.Hide();
        currentChest.RemoveStack(stack);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        ItemStack stack = view.GetStack();
        if (stack == null || stack.IsEmpty) return;

        ItemDragManager.Instance.BeginChestDrag(currentChest, stack, view, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ItemDragManager.Instance.UpdateDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ItemDragManager.Instance.EndDrag(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemDragManager.Instance != null && ItemDragManager.Instance.IsDragging) return;

        var stack = view.GetStack();
        if (stack == null || stack.IsEmpty) return;

        ItemTooltipUI.Instance.Show(stack, TooltipOwner.Chest);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.Instance.Hide();
    }
}
