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
    Common      // 일반 (흰색)
}

[CreateAssetMenu(menuName = "Item/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public int maxStack = 1;

    public ItemType itemType;
    public ItemRarity rarity;

    /*
    public int attackPower;
    public int defense;
    */
}
