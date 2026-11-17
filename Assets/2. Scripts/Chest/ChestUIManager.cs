using UnityEngine;

public class ChestUIManager : MonoBehaviour
{
    public static ChestUIManager Instance;

    [SerializeField] GameObject chestUI;
    [SerializeField] Transform itemSlotParent;
    [SerializeField] GameObject chestSlotPrefab;

    public bool IsChestOpen => chestUI.activeSelf;

    /// <summary>상자 패널 (툴팁 위치 기준).</summary>
    public RectTransform PanelRect => (RectTransform)chestUI.transform;
    public Chest CurrentChest { get; private set; }

    public bool IsShowing(Chest chest) => IsChestOpen && CurrentChest == chest;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (GameInput.Cancel.WasPressedThisFrame() && chestUI.activeSelf)
        {
            CloseChestUI();
        }
    }

    public void OpenChestUI(Chest chest)
    {
        CurrentChest = chest;
        chestUI.SetActive(true);

        RefreshUI(chest);
    }

    public void CloseChestUI()
    {
        CurrentChest = null;
        chestUI.SetActive(false);

        ItemTooltipUI.Instance.Hide(); // 체스트 UI 닫을 때 툴팁도 숨김
    }

    void RefreshUI(Chest chest)
    {
        foreach (Transform child in itemSlotParent)
            Destroy(child.gameObject);

        foreach (var stack in chest.currentItems)
        {
            GameObject slotObj = Instantiate(chestSlotPrefab, itemSlotParent);
            ChestSlotUI slotUI = slotObj.GetComponent<ChestSlotUI>();
            slotUI.Init(stack, chest);
        }
    }

    public void RefreshChestUI(Chest chest)
    {
        RefreshUI(chest);
    }

    /// <summary>화면 좌표가 열린 상자 패널 안인지.</summary>
    public bool ContainsScreenPoint(Vector2 screenPoint, Camera uiCamera)
    {
        return IsChestOpen &&
               RectTransformUtility.RectangleContainsScreenPoint((RectTransform)chestUI.transform, screenPoint, uiCamera);
    }
}