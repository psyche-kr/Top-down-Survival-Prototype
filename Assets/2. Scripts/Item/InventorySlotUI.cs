using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 인벤토리 한 칸. 툴팁, 드래그(칸 이동 / 바닥에 버리기), 상자에서 끌어온 아이템을 받는 칸 역할을 한다.
/// </summary>
public class InventorySlotUI :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    ItemSlotUI view;

    [Tooltip("칸 왼쪽 아래 단축키 번호 (1~9, 0)")]
    [SerializeField] TMPro.TMP_Text keyLabel;

    public int Index { get; private set; }

    void Awake()
    {
        view = GetComponent<ItemSlotUI>();
    }

    public void Setup(int index)
    {
        Index = index;

        if (keyLabel != null)
            keyLabel.text = ((index + 1) % 10).ToString();
    }

    public void SetData(ItemStack stack)
    {
        view.SetData(stack);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ItemStack stack = view.GetStack();
        if (stack == null || stack.IsEmpty) return;

        if (eventData.button != PointerEventData.InputButton.Right) return;

        // Ctrl + 우클릭: 1개씩 바로 바닥에 버리기
        if (GameInput.Modifier.IsPressed())
        {
            ItemDragManager.Instance.DropFromInventorySlot(Index, 1);

            // 남은 게 있으면 툴팁 개수를 갱신하고, 다 버렸으면 닫는다
            ItemStack left = view.GetStack();
            if (left != null && !left.IsEmpty) ItemTooltipUI.Instance.Show(left, TooltipOwner.Inventory);
            else ItemTooltipUI.Instance.Hide();
            return;
        }

        // TODO: 우클릭 사용 / 장착 (이터널 리턴 방식)
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        ItemStack stack = view.GetStack();
        if (stack == null || stack.IsEmpty) return;

        ItemDragManager.Instance.BeginInventoryDrag(Index, stack, view, eventData);
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

        ItemTooltipUI.Instance.Show(stack, TooltipOwner.Inventory);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.Instance.Hide();
    }
}
