using UnityEngine;

[CreateAssetMenu(menuName = "Item/Chest Data")]
public class ChestData : ScriptableObject
{
    public Item[] itemTable;

    [Header("Slot Count")]
    public int slotCount = 8;

    [Header("Random Item Count")]
    public int minItemCount = 1;
    public int maxItemCount = 8;
}