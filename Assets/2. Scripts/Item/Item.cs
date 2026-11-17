using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Consumable, // 소모
    Equipment, // 장비
    Material // 재료
}

public enum ItemRarity
{
    Mythic,     // 초월 (빨강)
    Legendary,  // 전설 (노랑)
    Epic,       // 영웅 (보라)
    Rare,       // 희귀 (파랑)
    Uncommon,   // 고급 (초록)
    Common      // 일반 (회색)
}

/// <summary>툴팁에 표시되는 세부 분류 (이터널 리턴 기준).</summary>
public enum ItemCategory
{
    Material,   // 재료
    Food,       // 음식
    Drink,      // 음료
    Weapon,     // 무기
    Chest,      // 옷
    Head,       // 머리
    Arm,        // 팔
    Leg,        // 신발
    Special     // 특수
}

/// <summary>툴팁 효과 줄. 예) 체력 재생 +660</summary>
[System.Serializable]
public struct ItemEffect
{
    public string label;
    public string value;

    public override string ToString() => $"{label} {value}";
}

public static class ItemRarityColor
{
    public static Color Get(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Mythic => new Color(1f, 0.32f, 0.32f),
        ItemRarity.Legendary => new Color(1f, 0.8f, 0.2f),
        ItemRarity.Epic => new Color(0.72f, 0.42f, 1f),
        ItemRarity.Rare => new Color(0.25f, 0.62f, 1f),
        ItemRarity.Uncommon => new Color(0.35f, 0.85f, 0.35f),
        _ => new Color(0.72f, 0.72f, 0.74f)
    };

    public static string GetName(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Mythic => "초월",
        ItemRarity.Legendary => "전설",
        ItemRarity.Epic => "영웅",
        ItemRarity.Rare => "희귀",
        ItemRarity.Uncommon => "고급",
        _ => "일반"
    };
}

public static class ItemCategoryName
{
    public static string Get(ItemCategory category) => category switch
    {
        ItemCategory.Food => "음식",
        ItemCategory.Drink => "음료",
        ItemCategory.Weapon => "무기",
        ItemCategory.Chest => "옷",
        ItemCategory.Head => "머리",
        ItemCategory.Arm => "팔",
        ItemCategory.Leg => "신발",
        ItemCategory.Special => "특수",
        _ => "재료"
    };
}

[CreateAssetMenu(menuName = "Item/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    [Tooltip("현재 툴팁에는 표시하지 않음 (이터널 리턴 툴팁 형식)")]
    public string description;
    public Sprite icon;
    public int maxStack = 1;

    public ItemType itemType;
    public ItemRarity rarity;

    [Tooltip("툴팁 분류 줄 (음식, 음료, 재료, 무기 ...)")]
    public ItemCategory category = ItemCategory.Material;

    [Tooltip("툴팁 효과 줄 (초록색). 예) 체력 재생 / +660")]
    public List<ItemEffect> effects = new List<ItemEffect>();
}
