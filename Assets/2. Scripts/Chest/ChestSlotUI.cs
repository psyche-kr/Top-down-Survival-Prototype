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

        // æ∆¿Ã≈€ »πµÊ ∞°¥…«—¡ˆ »Æ¿Œ (¿Œ∫•≈‰∏Æ ∞¯∞£ √º≈©)
        bool success = PlayerInventoryManager.Instance.AddItem(stack.item);
        if (!success) return;

        // æ∆¿Ã≈€ »πµÊΩ√ ΩΩ∑‘ π–±‚
        int index = currentChest.currentItems.IndexOf(stack); 
        if (index < 0) return;
        currentChest.currentItems.RemoveAt(index);

        // ΩΩ∑‘ πŒ »ƒø° ∫Û ΩΩ∑‘ √ﬂ∞°
        currentChest.currentItems.Add(new ItemStack()); 

        ChestUIManager.Instance.RefreshChestUI(currentChest); // UI ∞ªΩ≈
        ItemTooltipUI.Instance.Hide(); // ≈¯∆¡ º˚±‚±‚
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