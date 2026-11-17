using UnityEngine;

/// <summary>장비 칸 (이터널 리턴 순서: 무기, 옷, 머리, 팔, 신발).</summary>
public enum EquipSlot
{
    Weapon,
    Chest,
    Head,
    Arm,
    Leg
}

/// <summary>
/// 캐릭터가 장착한 아이템. 아직 장착 기능(우클릭 장착 등)은 없어서 인스펙터에서 직접 넣어 테스트한다.
/// 머리 위 HP바 위의 장비 등급 바가 이 값을 표시한다.
/// </summary>
public class PlayerEquipment : MonoBehaviour
{
    public const int SlotCount = 5;

    [Tooltip("0 무기, 1 옷, 2 머리, 3 팔, 4 신발")]
    [SerializeField] Item[] equipped = new Item[SlotCount];

    /// <summary>장비가 바뀌면 호출.</summary>
    public event System.Action Changed;

    public Item Get(EquipSlot slot) => equipped[(int)slot];

    /// <summary>빈 칸은 일반 등급으로 취급한다.</summary>
    public ItemRarity GetRarity(EquipSlot slot)
    {
        Item item = equipped[(int)slot];
        return item != null ? item.rarity : ItemRarity.Common;
    }

    public void Equip(EquipSlot slot, Item item)
    {
        equipped[(int)slot] = item;
        Changed?.Invoke();
    }

    void OnValidate()
    {
        if (equipped == null || equipped.Length != SlotCount)
            System.Array.Resize(ref equipped, SlotCount);

        Changed?.Invoke();
    }
}
