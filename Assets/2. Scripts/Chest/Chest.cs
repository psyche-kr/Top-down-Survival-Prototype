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
    public float GetInteractRange() => 0.6f;

    public void Interact(Character character)
    {
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
        ChestUIManager.Instance.CloseChestUI();
        isOpen = false;
    }
}