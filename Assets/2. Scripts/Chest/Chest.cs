using UnityEngine;
using System.Collections.Generic;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] ChestData data;

    public List<ItemStack> currentItems = new List<ItemStack>();

    Collider chestCollider;
    bool isOpen = false;
    Transform player;
    Vector3 openPlayerPosition;
    float positionTolerance = 0.1f;

    public ChestData GetData() => data;
    public Collider GetCollider() => chestCollider;
    // NavMesh가 상자 주변을 에이전트 반지름(0.5)만큼 깎아 내므로 그보다 넉넉하게 둔다
    public float GetInteractRange() => 1.0f;

    /// <summary>아직 이 상자에 들어 있는 스택인지 (이미 꺼낸 스택으로 복제되는 것 방지).</summary>
    public bool HasStack(ItemStack stack) => stack != null && !stack.IsEmpty && currentItems.Contains(stack);

    /// <summary>상자에서 아이템을 꺼낸다. 뒤 칸이 앞으로 당겨지고 마지막에 빈 칸이 붙는다.</summary>
    public bool RemoveStack(ItemStack stack)
    {
        int index = currentItems.IndexOf(stack);
        if (index < 0) return false;

        currentItems.RemoveAt(index);
        currentItems.Add(new ItemStack());

        if (ChestUIManager.Instance.IsShowing(this))
            ChestUIManager.Instance.RefreshChestUI(this);

        return true;
    }

    public void Interact(Character character)
    {
        // 이미 열려 있으면 다시 열지 않음 (UI 깜빡임 방지)
        if (ChestUIManager.Instance.IsShowing(this)) return;

        Open();
    }

    void Awake()
    {
        chestCollider = GetComponent<Collider>();
        player = GameObject.FindWithTag("Player").transform;

        InitializeSlots();
        GenerateRandomItems();
    }

    void InitializeSlots()
    {
        currentItems.Clear();

        for (int i = 0; i < data.slotCount; i++)
        {
            currentItems.Add(new ItemStack()); // 빈 슬롯
        }
    }

    void GenerateRandomItems()
    {
        int itemCount = Random.Range(data.minItemCount, data.maxItemCount + 1);

        for (int i = 0; i < itemCount && i < currentItems.Count; i++)
        {
            Item randomItem = data.itemTable[Random.Range(0, data.itemTable.Length)];
            currentItems[i] = new ItemStack(randomItem, 1);
        }
    }

    void Update()
    {
        if (isOpen && player != null)
        {
            if ((player.position - openPlayerPosition).sqrMagnitude >
                positionTolerance * positionTolerance)
            {
                Close();
            }
        }
    }

    void Open()
    {
        openPlayerPosition = player.position;
        ChestUIManager.Instance.OpenChestUI(this);
        isOpen = true;
    }

    void Close()
    {
        // 다른 상자 UI가 떠 있으면 건드리지 않음
        if (ChestUIManager.Instance.IsShowing(this))
            ChestUIManager.Instance.CloseChestUI();
        isOpen = false;
    }
}