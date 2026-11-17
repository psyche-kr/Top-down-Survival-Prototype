
public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [SerializeField] GameObject inventoryUI;
    [SerializeField] Transform itemSlotParent;
    [SerializeField] GameObject inventorySlotPrefab;

    void Awake() => Instance = this;

    void Update()
    {
        if (GameInput.ToggleInventory.WasPressedThisFrame())
        {
            if (inventoryUI.activeSelf)
                CloseInventory();
            else
                OpenInventory();
        }
    }

    public void OpenInventory()
    {
        inventoryUI.SetActive(true);
        RefreshUI();
    }

    public void CloseInventory()
    {
        inventoryUI.SetActive(false);

        if (ItemTooltipUI.Instance != null && 
            ItemTooltipUI.Instance.CurrentOwner == TooltipOwner.Inventory)
        {
            ItemTooltipUI.Instance.Hide(); 
        }
    }

    public void RefreshUI()
    {
        if(!inventoryUI.activeSelf) return; // 인벤토리가 닫혀있어도 갱신하려는거 방지

        foreach (Transform child in itemSlotParent)
            Destroy(child.gameObject);

        foreach (var stack in PlayerInventoryManager.Instance.items)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, itemSlotParent);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

            if (stack != null && stack.item != null)
                slotUI.Init(stack);
            else
                slotUI.Init(null);  // 빈 슬롯 처리
        }
    }
}