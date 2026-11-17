using UnityEngine;

/// <summary>
/// 로컬 플레이어의 입력을 읽어 Character에 명령을 전달한다.
/// (UnityEngine.InputSystem.PlayerInput과 이름이 겹치지 않도록 PlayerInputHandler로 명명)
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    Camera mainCamera;
    Character character;
    ObjectOutline currentOutline;
    [SerializeField] GameObject clickEffect;

    void Awake()
    {
        mainCamera = Camera.main;
        character = GetComponent<Character>();
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(GameInput.PointerPosition);
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit);

        // 오브젝트 외곽선
        if (hasHit && hit.collider.TryGetComponent(out ObjectOutline newOutline))
        {
            if (currentOutline != newOutline)
            {
                ClearOutline();
                newOutline.SetOutline(true);
                currentOutline = newOutline;
            }
        }
        else
        {
            ClearOutline();
        }

        // 이동 (우클릭을 누르고 있는 동안 계속 목적지 갱신)
        if (GameInput.Move.IsPressed() && hasHit)
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                character.MoveAndInteract(interactable);
            }
            else
            {
                character.MoveTo(hit.point);
            }
        }

        // 우클릭 이펙트
        if (GameInput.Move.WasPressedThisFrame() && hasHit)
        {
            // 살짝 위에 생성해서 지형과 겹치지 않게
            Instantiate(clickEffect, hit.point + Vector3.up * 0.03f, Quaternion.LookRotation(hit.normal));
        }

        // 정지
        if (GameInput.Stop.WasPressedThisFrame())
        {
            character.Stop();
        }

        // TODO: 공격 (GameInput.Attack)
    }

    void ClearOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.SetOutline(false);
            currentOutline = null;
        }
    }
}
