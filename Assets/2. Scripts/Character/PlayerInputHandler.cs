using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 로컬 플레이어의 입력을 읽어 Character에 명령을 전달한다.
/// (UnityEngine.InputSystem.PlayerInput과 이름이 겹치지 않도록 PlayerInputHandler로 명명)
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    Camera mainCamera;
    Character character;
    IHoverable currentHover;
    [SerializeField] GameObject clickEffect;

    void Awake()
    {
        mainCamera = Camera.main;
        character = GetComponent<Character>();
    }

    void Update()
    {
        // 정지 (UI 위에서도 동작)
        if (GameInput.Stop.WasPressedThisFrame())
        {
            character.Stop();
        }

        // UI 위에서는 이동/클릭 처리 안 함
        if (IsPointerOverUI())
        {
            SetHover(null);
            return;
        }

        // UI가 아닌 곳을 왼클릭하면 상자 닫기
        if (GameInput.Select.WasPressedThisFrame() && ChestUIManager.Instance != null)
        {
            ChestUIManager.Instance.CloseChestUI();
        }

        Ray ray = mainCamera.ScreenPointToRay(GameInput.PointerPosition);
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit);

        // 마우스를 올린 대상 강조 (오브젝트 테두리, 유닛 적/아군 색, 바닥 아이템 이름표)
        IHoverable newHover = null;
        if (hasHit) hit.collider.TryGetComponent(out newHover);
        SetHover(newHover);

        // 이동 (우클릭을 누르고 있는 동안 계속 목적지 갱신)
        if (GameInput.Move.IsPressed() && hasHit)
        {
            // 이터널 리턴처럼 우클릭을 누른 채 끌다가 오브젝트에 닿아도 다가가서 상호작용한다
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                character.MoveAndInteract(interactable);
            }
            else
            {
                character.MoveTo(hit.point);
            }
        }

        // 이동 명령 이펙트: 바닥을 우클릭했을 때만 (오브젝트·유닛 클릭에는 표시하지 않음)
        if (GameInput.Move.WasPressedThisFrame() && hasHit && clickEffect != null
            && hit.normal.y > 0.5f && !hit.collider.TryGetComponent<IInteractable>(out _))
        {
            Instantiate(clickEffect, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
        }

        // TODO: 공격 (GameInput.Attack)
    }

    void SetHover(IHoverable hover)
    {
        // 파괴된 오브젝트(주운 아이템)는 Unity null 비교로 걸러낸다
        if (currentHover is Object currentObj && currentObj == null)
            currentHover = null;

        if (currentHover == hover) return;

        currentHover?.SetHovered(false);
        currentHover = hover;
        currentHover?.SetHovered(true);
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        return EventSystem.current.IsPointerOverGameObject();
    }
}
