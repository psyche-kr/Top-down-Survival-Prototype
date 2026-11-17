using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TooltipOwner
{
    None,
    Inventory,
    Chest
}

/// <summary>
/// 이터널 리턴 형식의 아이템 툴팁.
///   머리: 등급 색 세로줄 | 이름 / 등급 / 분류 / 개수 | 아이콘
///   본문: 효과(초록) / 최대 중첩 수량
///   상위 아이템: 이 아이템을 재료로 쓰는 아이템 아이콘 (조합식이 있을 때만)
/// 위치: 인벤토리 아이템은 인벤토리 위에 고정, 상자 아이템은 마우스 커서의 왼쪽 위(띄울 때의 커서 위치 기준).
/// </summary>
public class ItemTooltipUI : MonoBehaviour
{
    public static ItemTooltipUI Instance;

    [SerializeField] RectTransform tooltipPanel;
    [SerializeField] RecipeDatabase recipeDatabase;

    [Header("Header")]
    [SerializeField] Image headerBackground;
    [SerializeField] Image rarityBar;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI rarityText;
    [SerializeField] TextMeshProUGUI categoryText;
    [SerializeField] TextMeshProUGUI amountText;
    [SerializeField] Image iconImage;

    [Header("Body")]
    [SerializeField] TextMeshProUGUI effectsText;
    [SerializeField] TextMeshProUGUI maxStackText;

    [Header("Upper Items")]
    [SerializeField] GameObject upperSection;
    [SerializeField] Transform upperGrid;
    [SerializeField] Image upperIconPrefab;

    [Header("Placement")]
    [Tooltip("인벤토리 패널 왼쪽 위 기준 오프셋 (툴팁 왼쪽 아래 모서리)")]
    [SerializeField] Vector2 inventoryOffset = new Vector2(-40f, 150f);
    [Tooltip("상자 아이템: 마우스 위치 기준 오프셋 (툴팁 오른쪽 아래 모서리가 커서의 왼쪽 위에 오도록)")]
    [SerializeField] Vector2 chestMouseOffset = new Vector2(-12f, 12f);

    readonly List<Image> upperIcons = new List<Image>();
    Canvas canvas;
    TooltipOwner currentOwner = TooltipOwner.None;

    public TooltipOwner CurrentOwner => currentOwner;

    void Awake()
    {
        Instance = this;
        // 이 컴포넌트는 Canvas 밖(UI Manager)에 있을 수 있으므로 패널 기준으로 찾는다
        canvas = tooltipPanel.GetComponentInParent<Canvas>(true).rootCanvas;
        if (upperIconPrefab != null) upperIconPrefab.gameObject.SetActive(false);
        Hide();
    }

    public void Show(ItemStack stack, TooltipOwner owner)
    {
        if (stack == null || stack.IsEmpty) return;

        Item item = stack.item;
        Color rarityColor = ItemRarityColor.Get(item.rarity);

        currentOwner = owner;
        tooltipPanel.gameObject.SetActive(true);

        // 머리
        nameText.text = item.itemName;
        rarityText.text = ItemRarityColor.GetName(item.rarity);
        rarityText.color = rarityColor;
        rarityBar.color = rarityColor;
        categoryText.text = ItemCategoryName.Get(item.category);
        amountText.text = $"{stack.amount} 개";
        iconImage.sprite = item.icon;
        headerBackground.color = HeaderTint(rarityColor);

        // 본문
        if (item.effects != null && item.effects.Count > 0)
        {
            effectsText.gameObject.SetActive(true);
            var lines = new List<string>();
            foreach (var effect in item.effects) lines.Add(effect.ToString());
            effectsText.text = string.Join("\n", lines);
        }
        else
        {
            effectsText.gameObject.SetActive(false);
        }

        maxStackText.text = $"최대 중첩 수량 : {item.maxStack}";

        // 상위 아이템
        List<Item> upper = recipeDatabase != null ? recipeDatabase.GetUpperItems(item) : new List<Item>();
        upperSection.SetActive(upper.Count > 0);
        SetUpperIcons(upper);

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);
        Place(owner);
    }

    public void Hide()
    {
        tooltipPanel.gameObject.SetActive(false);
        currentOwner = TooltipOwner.None;
    }

    void SetUpperIcons(List<Item> items)
    {
        while (upperIcons.Count < items.Count)
            upperIcons.Add(Instantiate(upperIconPrefab, upperGrid));

        for (int i = 0; i < upperIcons.Count; i++)
        {
            bool used = i < items.Count;
            upperIcons[i].gameObject.SetActive(used);
            if (!used) continue;

            // 배경은 등급 색을 어둡게, 자식 아이콘에 아이템 이미지
            Color c = ItemRarityColor.Get(items[i].rarity);
            upperIcons[i].color = new Color(c.r * 0.45f, c.g * 0.45f, c.b * 0.45f, 1f);
            Image icon = upperIcons[i].transform.GetChild(0).GetComponent<Image>();
            icon.sprite = items[i].icon;
        }
    }

    void Place(TooltipOwner owner)
    {
        float scale = canvas.scaleFactor;
        var corners = new Vector3[4]; // 0 좌하, 1 좌상, 2 우상, 3 우하

        if (owner == TooltipOwner.Inventory && InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.PanelRect.GetWorldCorners(corners);
            tooltipPanel.pivot = new Vector2(0f, 0f);
            tooltipPanel.position = corners[1] + (Vector3)(inventoryOffset * scale);
        }
        else if (owner == TooltipOwner.Chest)
        {
            // 이터널 리턴처럼 마우스 커서의 왼쪽 위에 띄운다 (칸을 옮길 때마다 그 순간의 커서 위치 기준)
            tooltipPanel.pivot = new Vector2(1f, 0f);
            tooltipPanel.position = (Vector3)GameInput.PointerPosition + (Vector3)(chestMouseOffset * scale);
        }

        KeepOnScreen();
    }

    void KeepOnScreen()
    {
        var corners = new Vector3[4];
        tooltipPanel.GetWorldCorners(corners);

        Vector3 shift = Vector3.zero;
        if (corners[0].x < 0) shift.x = -corners[0].x;
        if (corners[2].x > Screen.width) shift.x = Screen.width - corners[2].x;
        if (corners[0].y < 0) shift.y = -corners[0].y;
        if (corners[2].y > Screen.height) shift.y = Screen.height - corners[2].y;

        tooltipPanel.position += shift;
    }

    // 머리 배경: 등급 색을 아주 어둡게 섞은 남색 (희귀=푸른빛, 일반=회색빛)
    static Color HeaderTint(Color rarity)
    {
        Color baseColor = new Color(0.11f, 0.13f, 0.17f, 0.97f);
        return Color.Lerp(baseColor, rarity, 0.18f) * new Color(1, 1, 1, 0.97f);
    }
}
