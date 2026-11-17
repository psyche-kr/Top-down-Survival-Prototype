using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance;

    const int INVENTORY_SIZE = 10;

    public List<ItemStack> items = new List<ItemStack>();

    [SerializeField] GameObject WarningPanel;

    Coroutine warningCoroutine;

    void Awake() => Instance = this;

    public bool AddItem(Item item, int count = 1)
    {
        if (item == null || count <= 0) return false;

        if (IsInventoryFull(item, count))
        {
            ShowWarning();
            return false;
        }

        int remaining = count;

        // 기존 스택 먼저 채우기
        foreach (var stack in items)
        {
            if (stack == null || stack.IsEmpty) continue;

            if (stack.item.itemName != item.itemName) continue;

            int spaceLeft = item.maxStack - stack.amount;
            if (spaceLeft <= 0) continue;

            int addAmount = Mathf.Min(spaceLeft, remaining);

            stack.amount += addAmount;
            remaining -= addAmount;

            if (remaining <= 0)
                break;
        }

        // 남은 수량으로 새 스택 생성
        while (remaining > 0)
        {
            int addAmount = Mathf.Min(item.maxStack, remaining);

            items.Add(new ItemStack(item, addAmount));

            remaining -= addAmount;
        }

        InventoryUIManager.Instance.RefreshUI();
        return true;
    }

    bool IsInventoryFull(Item item, int count)
    {
        int remaining = count;

        foreach (var stack in items)
        {
            if (stack == null || stack.IsEmpty) continue;

            if (stack.item.itemName != item.itemName) continue;

            int spaceLeft = item.maxStack - stack.amount;

            if (spaceLeft > 0)
                remaining -= Mathf.Min(spaceLeft, remaining);

            if (remaining <= 0)
                return false;
        }

        int emptySlots = INVENTORY_SIZE - items.Count;

        return remaining > 0 && emptySlots <= 0;
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