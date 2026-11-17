using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 바닥에 버려진 아이템. 3D 모델 없이 바닥의 아이콘(월드)과 이름표(화면 UI)로 표시한다.
/// 이름표는 오브젝트보다 앞에 보이고, 우클릭·Alt+좌클릭으로 주울 수 있다(DroppedItemNameTag).
/// 아이콘 쪽은 우클릭하면 캐릭터가 다가가서 줍는다(IInteractable).
/// </summary>
[RequireComponent(typeof(Collider))]
public class DroppedItem : MonoBehaviour, IInteractable, IHoverable
{
    /// <summary>바닥에 있는 아이템 전체 (버릴 때 겹치지 않는 자리를 찾는 데 사용).</summary>
    public static readonly List<DroppedItem> All = new List<DroppedItem>();

    [SerializeField] Transform label;
    [SerializeField] Image iconImage;
    [SerializeField] DroppedItemNameTag nameTagPrefab;
    [SerializeField] float interactRange = 0.8f;

    [Header("Name Background")]
    [SerializeField] Color normalBackground = new Color(0.05f, 0.08f, 0.1f, 0.45f);
    [SerializeField] Color hoverBackground = new Color(0.02f, 0.03f, 0.05f, 0.85f);

    ItemStack stack;
    Collider itemCollider;
    Camera mainCamera;
    DroppedItemNameTag nameTag;
    bool worldHovered, tagHovered;

    void Awake()
    {
        itemCollider = GetComponent<Collider>();
        mainCamera = Camera.main;
    }

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    void OnDestroy()
    {
        if (nameTag != null) Destroy(nameTag.gameObject);
    }

    public void Init(ItemStack itemStack)
    {
        stack = itemStack;
        iconImage.sprite = stack.item.icon;
        name = $"DroppedItem ({stack.item.itemName})";

        RectTransform layer = WorldUILayer.Get();
        if (nameTagPrefab != null && layer != null)
        {
            nameTag = Instantiate(nameTagPrefab, layer);
            string text = stack.amount > 1 ? $"{stack.item.itemName} ({stack.amount})" : stack.item.itemName;
            nameTag.Bind(this, text, ItemRarityColor.Get(stack.item.rarity));
        }
        RefreshHover();
    }

    void LateUpdate()
    {
        // 쿼터뷰 카메라와 같은 방향을 보게 해서 아이콘이 항상 정면으로 보이게
        if (label != null && mainCamera != null)
            label.rotation = mainCamera.transform.rotation;
    }

    // 월드 쪽(아이콘 콜라이더) 호버 - PlayerInputHandler가 호출
    public void SetHovered(bool hovered)
    {
        worldHovered = hovered;
        RefreshHover();
    }

    // 이름표 호버 - DroppedItemNameTag가 호출
    public void SetTagHovered(bool hovered)
    {
        tagHovered = hovered;
        RefreshHover();
    }

    void RefreshHover()
    {
        if (nameTag != null)
            nameTag.SetVisual(worldHovered || tagHovered, normalBackground, hoverBackground);
    }

    public Collider GetCollider() => itemCollider;
    public float GetInteractRange() => interactRange;

    public void Interact(Character character)
    {
        if (stack == null || stack.IsEmpty) return;

        // 인벤토리가 가득 차면 경고가 뜨고 바닥에 그대로 남는다
        if (PlayerInventoryManager.Instance.AddItem(stack.item, stack.amount))
            Destroy(gameObject);
    }

    /// <summary>우클릭: 다가가서 줍기.</summary>
    public void RequestWalkAndPickup()
    {
        Character player = FindPlayer();
        if (player != null) player.MoveAndInteract(this);
    }

    /// <summary>Alt+좌클릭: 습득 범위 안에 있을 때만 바로 줍기 (범위 밖이면 아무 일도 없음).</summary>
    public bool TryPickupInRange()
    {
        Character player = FindPlayer();
        if (player == null || !player.IsInRange(this)) return false;

        Interact(player);
        return true;
    }

    static Character FindPlayer()
    {
        GameObject go = GameObject.FindWithTag("Player");
        return go != null ? go.GetComponent<Character>() : null;
    }
}
