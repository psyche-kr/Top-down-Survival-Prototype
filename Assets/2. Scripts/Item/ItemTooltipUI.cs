using TMPro;
using UnityEngine;

public enum TooltipOwner
{
    None,
    Inventory,
    Chest
}

public class ItemTooltipUI : MonoBehaviour
{
    public static ItemTooltipUI Instance;

    [SerializeField] GameObject tooltipPanel;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI itemTypeText;

    TooltipOwner currentOwner = TooltipOwner.None;
    bool isFollowing = false;

    [SerializeField] Vector3 tooltipOffset = new Vector3();

    public TooltipOwner CurrentOwner => currentOwner;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    void Update()
    {
        if (isFollowing)
        {
            tooltipPanel.transform.position = (Vector3)GameInput.PointerPosition + tooltipOffset;
        }
    }

    public void Show(Item item, TooltipOwner owner)
    {
        tooltipPanel.SetActive(true);

        isFollowing = true;
        currentOwner = owner;

        nameText.text = item.itemName;
        descriptionText.text = item.description;

        SetNameColor(item.rarity);
        SetItemTypeText(item.itemType);
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);

        isFollowing = false;
        currentOwner = TooltipOwner.None;
    }

    void SetNameColor(ItemRarity rarity)
    {
        Color color = rarity switch
        {
            ItemRarity.Mythic => Color.red,
            ItemRarity.Legendary => Color.yellow,
            ItemRarity.Epic => new Color(0.6f, 0f, 1f),
            ItemRarity.Rare => Color.blue,
            ItemRarity.Uncommon => Color.green,
            _ => Color.white
        };
        nameText.color = color;
    }

    void SetItemTypeText(ItemType type)
    {
        itemTypeText.text = type switch
        {
            ItemType.Consumable => "소모품",
            ItemType.Equipment => "장비",
            ItemType.Material => "재료",
            _ => ""
        };
        itemTypeText.color = new Color(0.7f, 0.7f, 0.75f);
    }
}
