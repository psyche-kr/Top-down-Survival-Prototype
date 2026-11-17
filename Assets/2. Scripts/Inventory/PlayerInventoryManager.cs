using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 인벤토리 데이터. 이터널 리턴처럼 칸 위치가 고정된 10칸(2x5).
/// 아이템을 빼도 다른 칸이 당겨지지 않고, 새 아이템은 같은 아이템 스택을 먼저 채운 뒤 앞쪽 빈 칸에 들어간다.
/// </summary>
public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance;

    public const int SIZE = 10;

    [SerializeField] List<ItemStack> slots = new List<ItemStack>();

    [SerializeField] GameObject WarningPanel;

    Coroutine warningCoroutine;

    public int Size => SIZE;

    void Awake()
    {
        Instance = this;

        slots.Clear();
        for (int i = 0; i < SIZE; i++)
            slots.Add(new ItemStack());
    }

    public ItemStack GetSlot(int index) => slots[index];

    /// <summary>같은 아이템 스택을 먼저 채우고 남으면 앞쪽 빈 칸에 넣는다. 전부 못 넣으면 아무것도 넣지 않는다.</summary>
    public bool AddItem(Item item, int count = 1)
    {
        if (item == null || count <= 0) return false;

        if (!CanAdd(item, count))
        {
            ShowWarning();
            return false;
        }

        int remaining = count;

        // 기존 스택 먼저 채우기
        foreach (var stack in slots)
        {
            if (stack.IsEmpty || stack.item != item) continue;

            int add = Mathf.Min(item.maxStack - stack.amount, remaining);
            if (add <= 0) continue;

            stack.amount += add;
            remaining -= add;
            if (remaining <= 0) break;
        }

        // 앞쪽 빈 칸부터 채우기
        for (int i = 0; i < slots.Count && remaining > 0; i++)
        {
            if (!slots[i].IsEmpty) continue;

            int add = Mathf.Min(item.maxStack, remaining);
            slots[i].Set(item, add);
            remaining -= add;
        }

        Refresh();
        return true;
    }

    /// <summary>
    /// 지정한 칸에 넣는다(드래그로 놓은 칸). 빈 칸이면 그 칸에, 같은 아이템이고 공간이 있으면 합치고,
    /// 다른 아이템이 있으면 AddItem처럼 다른 빈 칸을 찾는다.
    /// </summary>
    public bool TryPlaceAt(int index, Item item, int count)
    {
        if (item == null || count <= 0) return false;

        ItemStack slot = slots[index];

        if (slot.IsEmpty && count <= item.maxStack)
        {
            slot.Set(item, count);
            Refresh();
            return true;
        }

        if (!slot.IsEmpty && slot.item == item && slot.amount + count <= item.maxStack)
        {
            slot.amount += count;
            Refresh();
            return true;
        }

        return AddItem(item, count);
    }

    /// <summary>인벤토리 안에서 칸 이동. 같은 아이템이면 합치고, 아니면 자리를 바꾼다.</summary>
    public void MoveSlot(int from, int to)
    {
        if (from == to) return;

        ItemStack a = slots[from];
        ItemStack b = slots[to];
        if (a.IsEmpty) return;

        if (!b.IsEmpty && b.item == a.item && b.amount < b.item.maxStack)
        {
            int move = Mathf.Min(b.item.maxStack - b.amount, a.amount);
            b.amount += move;
            a.amount -= move;
            if (a.amount <= 0) a.Clear();
        }
        else
        {
            slots[from] = b;
            slots[to] = a;
        }

        Refresh();
    }

    /// <summary>칸을 비우고 들어 있던 아이템을 돌려준다(바닥에 버릴 때 사용).</summary>
    public ItemStack RemoveAt(int index)
    {
        ItemStack slot = slots[index];
        if (slot.IsEmpty) return null;

        ItemStack removed = new ItemStack(slot.item, slot.amount);
        slot.Clear();

        Refresh();
        return removed;
    }

    /// <summary>칸에서 amount개만 빼서 돌려준다. 남은 개수가 0이 되면 칸을 비운다.</summary>
    public ItemStack RemoveAt(int index, int amount)
    {
        ItemStack slot = slots[index];
        if (slot.IsEmpty || amount <= 0) return null;

        int take = Mathf.Min(amount, slot.amount);
        ItemStack removed = new ItemStack(slot.item, take);

        slot.amount -= take;
        if (slot.amount <= 0) slot.Clear();

        Refresh();
        return removed;
    }

    bool CanAdd(Item item, int count)
    {
        int space = 0;

        foreach (var stack in slots)
        {
            if (stack.IsEmpty)
                space += item.maxStack;
            else if (stack.item == item)
                space += item.maxStack - stack.amount;

            if (space >= count) return true;
        }

        return false;
    }

    void Refresh()
    {
        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.RefreshUI();
    }

    void ShowWarning()
    {
        if (WarningPanel == null) return;

        WarningPanel.SetActive(true);

        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);

        warningCoroutine = StartCoroutine(HideWarning(2f));
    }

    IEnumerator HideWarning(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (WarningPanel != null)
            WarningPanel.SetActive(false);
    }
}
