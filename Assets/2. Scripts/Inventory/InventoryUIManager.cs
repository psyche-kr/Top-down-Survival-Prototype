using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 화면 오른쪽 아래에 항상 떠 있는 인벤토리 UI. 슬롯 10개를 한 번만 만들고 데이터만 갱신한다.
/// (드래그 중에 슬롯이 파괴되지 않도록 다시 만들지 않는다)
/// </summary>
public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance;

    [SerializeField] GameObject inventoryUI;
    [SerializeField] Transform itemSlotParent;
    [SerializeField] GameObject inventorySlotPrefab;

    readonly List<InventorySlotUI> slots = new List<InventorySlotUI>();

    /// <summary>인벤토리 패널 (툴팁 위치 기준).</summary>
    public RectTransform PanelRect => (RectTransform)inventoryUI.transform;

    void Awake() => Instance = this;

    void Start()
    {
        inventoryUI.SetActive(true);
        BuildSlots();
        RefreshUI();
    }

    void BuildSlots()
    {
        foreach (Transform child in itemSlotParent)
            Destroy(child.gameObject);

        slots.Clear();

        for (int i = 0; i < PlayerInventoryManager.Instance.Size; i++)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, itemSlotParent);
            slotObj.name = $"InventorySlot {i}";

            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            slotUI.Setup(i);
            slots.Add(slotUI);
        }
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slots.Count; i++)
            slots[i].SetData(PlayerInventoryManager.Instance.GetSlot(i));
    }

    /// <summary>화면 좌표가 인벤토리 패널 안인지.</summary>
    public bool ContainsScreenPoint(Vector2 screenPoint, Camera uiCamera)
    {
        return inventoryUI.activeInHierarchy &&
               RectTransformUtility.RectangleContainsScreenPoint((RectTransform)inventoryUI.transform, screenPoint, uiCamera);
    }
}
