using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 바닥 아이템의 이름표. 화면 UI(WorldUI 층)에서 아이템 위치를 따라가므로 상자 등 3D 오브젝트보다 앞에 보인다.
///   마우스 오버: 배경이 짙어지고 모서리 표시
///   우클릭: 다가가서 줍기
///   Alt + 좌클릭: 습득 범위 안이면 바로 줍기
/// </summary>
[DefaultExecutionOrder(100)] // 카메라 이동 뒤에 화면 위치 계산
public class DroppedItemNameTag : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Image background;
    [SerializeField] GameObject hoverCorners;
    [Tooltip("아이템 위치 기준 이름표 높이")]
    [SerializeField] Vector3 worldOffset = new Vector3(0f, 1.32f, 0f);

    DroppedItem owner;
    RectTransform rect;
    Camera mainCamera;

    void Awake()
    {
        rect = (RectTransform)transform;
        mainCamera = Camera.main;
    }

    public void Bind(DroppedItem item, string text, Color textColor)
    {
        owner = item;
        nameText.text = text;
        nameText.color = textColor;
        LateUpdate();
    }

    public void SetVisual(bool hovered, Color normalBackground, Color hoverBackground)
    {
        background.color = hovered ? hoverBackground : normalBackground;
        if (hoverCorners != null) hoverCorners.SetActive(hovered);
    }

    void LateUpdate()
    {
        if (owner == null || mainCamera == null) return;

        Vector3 screen = mainCamera.WorldToScreenPoint(owner.transform.position + worldOffset);
        rect.position = screen.z > 0f
            ? new Vector3(screen.x, screen.y, 0f)
            : new Vector3(-10000f, -10000f, 0f);
    }

    public void OnPointerEnter(PointerEventData eventData) => owner?.SetTagHovered(true);
    public void OnPointerExit(PointerEventData eventData) => owner?.SetTagHovered(false);

    public void OnPointerClick(PointerEventData eventData)
    {
        if (owner == null) return;

        if (eventData.button == PointerEventData.InputButton.Right)
            owner.RequestWalkAndPickup();
        else if (eventData.button == PointerEventData.InputButton.Left && GameInput.AltModifier.IsPressed())
            owner.TryPickupInRange();
    }
}
