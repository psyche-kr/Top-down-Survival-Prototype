using UnityEngine;

public class ChestUIManager : MonoBehaviour
{
    public static ChestUIManager Instance;

    [SerializeField] GameObject chestUI;
    [SerializeField] Transform itemSlotParent;
    [SerializeField] GameObject chestSlotPrefab;

    public bool IsChestOpen => chestUI.activeSelf;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && chestUI.activeSelf)
        {
            CloseChestUI();
        }
    }

    public void OpenChestUI(Chest chest)
    {
        chestUI.SetActive(true);

        RefreshUI(chest);
    }

    public void CloseChestUI()
    {
        chestUI.SetActive(false);

        ItemTooltipUI.Instance.Hide(); // Ã¼½ºÆ® UI ´ÝÀ» ¶§ ÅøÆÁµµ ¼û±è
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
}