using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 아이템 드래그 처리. 드래그하는 동안 마우스를 따라다니는 아이콘(고스트)을 띄우고,
/// 놓은 위치에 따라 아이템을 옮긴다. 데이터는 놓는 순간에만 바뀐다.
///
/// 상자에서 끌었을 때
///   - 인벤토리 칸 위: 그 칸으로 이동 (칸이 차 있으면 같은 아이템은 합치고, 아니면 빈 칸으로)
///   - 인벤토리 패널 위: 빈 칸으로 이동
///   - 상자 UI 위 / 다른 UI 위: 원래 자리로 돌아감
///   - 그 밖(월드): 바닥에 버림
/// 인벤토리에서 끌었을 때
///   - 다른 인벤토리 칸 위: 자리 바꾸기 (같은 아이템이면 합치기)
///   - UI 위: 원래 자리로 돌아감
///   - 그 밖(월드): 바닥에 버림
/// </summary>
public class ItemDragManager : MonoBehaviour
{
    public static ItemDragManager Instance { get; private set; }

    [SerializeField] Canvas rootCanvas;
    [SerializeField] DroppedItem droppedItemPrefab;
    [SerializeField] Vector2 ghostSize = new Vector2(100f, 56f);
    [Tooltip("버린 아이템끼리 최소한 떨어져 있어야 하는 거리")]
    [SerializeField] float dropSpacing = 1.4f;
    [Tooltip("발밑이 차 있을 때 바깥으로 찾아볼 고리 수")]
    [SerializeField] int dropSearchRings = 4;

    RectTransform ghost;
    Image ghostImage;

    ItemStack dragStack;
    ItemSlotUI sourceView;
    Chest sourceChest;
    int sourceInventoryIndex = -1;

    Transform player;

    public bool IsDragging => dragStack != null;

    Camera UICamera => rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

    void Awake()
    {
        Instance = this;

        if (rootCanvas == null)
            rootCanvas = FindFirstObjectByType<Canvas>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        CreateGhost();
    }

    void CreateGhost()
    {
        var go = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        ghost = go.GetComponent<RectTransform>();
        ghost.SetParent(rootCanvas.transform, false);
        ghost.sizeDelta = ghostSize;

        ghostImage = go.GetComponent<Image>();
        ghostImage.raycastTarget = false;
        ghostImage.preserveAspect = true;

        // 놓을 때 아래에 있는 칸을 찾아야 하므로 고스트는 레이캐스트를 막지 않는다
        var group = go.GetComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        go.SetActive(false);
    }

    public void BeginChestDrag(Chest chest, ItemStack stack, ItemSlotUI view, PointerEventData eventData)
    {
        sourceChest = chest;
        sourceInventoryIndex = -1;
        Begin(stack, view, eventData);
    }

    public void BeginInventoryDrag(int index, ItemStack stack, ItemSlotUI view, PointerEventData eventData)
    {
        sourceChest = null;
        sourceInventoryIndex = index;
        Begin(stack, view, eventData);
    }

    void Begin(ItemStack stack, ItemSlotUI view, PointerEventData eventData)
    {
        dragStack = stack;
        sourceView = view;
        sourceView.SetDimmed(true);

        ghostImage.sprite = stack.item.icon;
        ghost.gameObject.SetActive(true);
        ghost.SetAsLastSibling();

        if (ItemTooltipUI.Instance != null)
            ItemTooltipUI.Instance.Hide();

        UpdateDrag(eventData);
    }

    // 안전장치: 드래그 중에 원본 칸이 사라지거나(상자 갱신·닫기) 버튼이 이미 떼어졌는데
    // OnEndDrag가 오지 않으면 고스트가 화면에 남으므로, 데이터는 건드리지 않고 드래그만 취소한다.
    // EventSystem은 Update에서 OnEndDrag를 보내므로, 정상적인 놓기를 가로채지 않도록 LateUpdate에서 확인한다.
    void LateUpdate()
    {
        if (!IsDragging) return;

        bool sourceGone = sourceView == null;
        bool released = !GameInput.Select.IsPressed();
        if (sourceGone || released) CancelDrag();
    }

    /// <summary>놓지 않고 드래그를 취소한다. 아이템은 원래 자리에 그대로 있다.</summary>
    public void CancelDrag()
    {
        if (sourceView != null) sourceView.SetDimmed(false);
        dragStack = null;
        sourceView = null;
        sourceChest = null;
        sourceInventoryIndex = -1;
        ghost.gameObject.SetActive(false);
    }

    public void UpdateDrag(PointerEventData eventData)
    {
        if (!IsDragging) return;

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                (RectTransform)rootCanvas.transform, eventData.position, UICamera, out Vector3 worldPoint))
        {
            ghost.position = worldPoint;
        }
    }

    public void EndDrag(PointerEventData eventData)
    {
        if (!IsDragging) return;

        ItemStack stack = dragStack;
        Chest chest = sourceChest;
        int fromIndex = sourceInventoryIndex;

        if (sourceView != null) sourceView.SetDimmed(false);
        dragStack = null;
        sourceView = null;
        sourceChest = null;
        sourceInventoryIndex = -1;
        ghost.gameObject.SetActive(false);

        // 놓은 위치 판정
        GameObject hitUI = eventData.pointerCurrentRaycast.gameObject;
        InventorySlotUI targetSlot = hitUI != null ? hitUI.GetComponentInParent<InventorySlotUI>() : null;

        bool overInventory = targetSlot != null ||
                             InventoryUIManager.Instance.ContainsScreenPoint(eventData.position, UICamera);
        bool overChest = ChestUIManager.Instance != null &&
                         ChestUIManager.Instance.ContainsScreenPoint(eventData.position, UICamera);
        bool overOtherUI = hitUI != null;

        if (chest != null)
            DropFromChest(chest, stack, targetSlot, overInventory, overChest || overOtherUI);
        else if (fromIndex >= 0)
            DropFromInventory(fromIndex, targetSlot, overInventory || overChest || overOtherUI);
    }

    void DropFromChest(Chest chest, ItemStack stack, InventorySlotUI targetSlot, bool overInventory, bool overUI)
    {
        PlayerInventoryManager inventory = PlayerInventoryManager.Instance;

        if (!chest.HasStack(stack)) return;

        if (overInventory)
        {
            bool success = targetSlot != null
                ? inventory.TryPlaceAt(targetSlot.Index, stack.item, stack.amount)
                : inventory.AddItem(stack.item, stack.amount);

            // 인벤토리가 가득 차면 경고가 뜨고 아이템은 상자에 남는다
            if (success) chest.RemoveStack(stack);
            return;
        }

        // 상자 안이나 다른 UI 위면 원래 자리로 (데이터를 건드리지 않았으므로 그대로 둔다)
        if (overUI) return;

        ItemStack dropped = new ItemStack(stack.item, stack.amount);
        chest.RemoveStack(stack);
        SpawnOnGround(dropped);
    }

    void DropFromInventory(int fromIndex, InventorySlotUI targetSlot, bool overUI)
    {
        PlayerInventoryManager inventory = PlayerInventoryManager.Instance;

        if (targetSlot != null)
        {
            inventory.MoveSlot(fromIndex, targetSlot.Index);
            return;
        }

        if (overUI) return;

        DropInventorySlot(fromIndex);
    }

    /// <summary>인벤토리 칸의 아이템을 통째로 바닥에 버린다 (드래그로 UI 밖에 놓았을 때).</summary>
    public void DropInventorySlot(int index)
    {
        ItemStack removed = PlayerInventoryManager.Instance.RemoveAt(index);
        if (removed != null) SpawnOnGround(removed);
    }

    /// <summary>인벤토리 칸에서 amount개만 바닥에 버린다 (Ctrl+우클릭은 1개씩).</summary>
    public void DropFromInventorySlot(int index, int amount)
    {
        ItemStack removed = PlayerInventoryManager.Instance.RemoveAt(index, amount);
        if (removed != null) SpawnOnGround(removed);
    }

    /// <summary>캐릭터 발밑에 아이템을 떨어뜨린다. 이미 아이템이 있으면 겹치지 않는 옆자리를 찾는다.</summary>
    public void SpawnOnGround(ItemStack stack)
    {
        if (droppedItemPrefab == null)
        {
            Debug.LogWarning("ItemDragManager: droppedItemPrefab이 비어 있어 아이템을 바닥에 놓을 수 없습니다.");
            return;
        }

        Vector3 origin = player != null ? player.position : Vector3.zero;
        Vector3 pos = FindDropPosition(origin);

        DroppedItem dropped = Instantiate(droppedItemPrefab, pos, Quaternion.identity);
        dropped.Init(stack);
    }

    /// <summary>발밑부터 시작해 바깥 고리로 넓혀 가며 다른 바닥 아이템과 겹치지 않는 NavMesh 위 지점을 찾는다.</summary>
    Vector3 FindDropPosition(Vector3 origin)
    {
        // 캐릭터가 서 있는 NavMesh 지점. 후보는 여기서 걸어갈 수 있어야 한다
        // (상자 옆면 같은 곳에 생긴 작은 NavMesh 조각에 떨어지면 줍지 못함)
        bool hasStart = TrySample(origin, out Vector3 start);

        if (hasStart && IsFree(start))
            return start;

        Vector3 pos;

        for (int ring = 1; ring <= dropSearchRings; ring++)
        {
            float radius = dropSpacing * ring;
            int count = 6 * ring; // 바깥 고리일수록 후보를 많이
            float startAngle = ring * 30f; // 고리마다 시작 각도를 어긋나게

            for (int i = 0; i < count; i++)
            {
                float angle = (startAngle + 360f * i / count) * Mathf.Deg2Rad;
                Vector3 candidate = origin + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;

                if (TrySample(candidate, out pos) && IsFree(pos) && (!hasStart || IsReachable(start, pos)))
                    return pos;
            }
        }

        // 빈자리를 못 찾으면 발밑에 겹쳐서 놓는다
        return hasStart ? start : origin;
    }

    NavMeshPath reachPath; // 필드 초기화에서 만들 수 없어 처음 쓸 때 만든다

    bool IsReachable(Vector3 from, Vector3 to)
    {
        if (reachPath == null) reachPath = new NavMeshPath();
        return NavMesh.CalculatePath(from, to, NavMesh.AllAreas, reachPath) &&
               reachPath.status == NavMeshPathStatus.PathComplete;
    }

    static bool TrySample(Vector3 point, out Vector3 result)
    {
        // 캐릭터 기준점이 바닥보다 1m 정도 위에 있으므로 여유 있게 2m까지 찾는다
        if (NavMesh.SamplePosition(point, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = point;
        return false;
    }

    bool IsFree(Vector3 point)
    {
        float sqrSpacing = dropSpacing * dropSpacing;

        foreach (var item in DroppedItem.All)
        {
            Vector3 diff = item.transform.position - point;
            diff.y = 0f;
            if (diff.sqrMagnitude < sqrSpacing) return false;
        }

        return true;
    }
}
