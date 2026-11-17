using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChestSlotUI : 
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    ItemSlotUI view;
    Button button;
    Chest currentChest;

    [SerializeField] Sprite filledBackground;
    [SerializeField] Sprite emptyBackground;

    Image backgroundImage;

    void Awake()
    {
        view = GetComponent<ItemSlotUI>();
        button = GetComponent<Button>();
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

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        ItemStack stack = view.GetStack();

        if (stack == null || stack.IsEmpty)
            return;

        // 아이템 획득 가능한지 확인 (인벤토리 공간 체크)
        bool success = PlayerInventoryManager.Instance.AddItem(stack.item);
        if (!success) return;

        // 아이템 획득시 슬롯 밀기
        int index = currentChest.currentItems.IndexOf(stack); 
        if (index < 0) return;
        currentChest.currentItems.RemoveAt(index);

        // 슬롯 민 후에 빈 슬롯 추가
        currentChest.currentItems.Add(new ItemStack()); 

        ChestUIManager.Instance.RefreshChestUI(currentChest); // UI 갱신
        ItemTooltipUI.Instance.Hide(); // 툴팁 숨기기
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var stack = view.GetStack();

        if (stack == null || stack.IsEmpty) return;

        ItemTooltipUI.Instance.Show(stack.item, TooltipOwner.Chest);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.Instance.Hide();
    }
}