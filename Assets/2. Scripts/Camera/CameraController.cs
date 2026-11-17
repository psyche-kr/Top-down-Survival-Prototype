using UnityEngine;

/// <summary>
/// 쿼터뷰 카메라. 캐릭터(NavMeshAgent)가 이번 프레임 이동을 끝낸 뒤 따라가도록 LateUpdate에서 움직인다.
/// (Update에서 따라가면 이동 전/후 위치를 번갈아 잡아 Space 따라가기 중 화면이 떨린다)
/// 머리 위 UI 등 카메라 위치로 화면 좌표를 계산하는 스크립트보다 먼저 실행되도록 실행 순서를 앞당긴다.
/// </summary>
[DefaultExecutionOrder(-100)]
public class CameraController : MonoBehaviour
{
    [SerializeField, Tooltip("카메라 이동속도")] float moveSpeed = 50f;
    [SerializeField, Tooltip("가장자리 경계선 크기")] float borderSize = 5f;

    // 나중에 맵 크기에 따라 변경할 것
    [SerializeField, Tooltip("최소 이동 가능 범위")] Vector2 minLimit;
    [SerializeField, Tooltip("최소 이동 가능 범위")] Vector2 maxLimit;

    [SerializeField, Tooltip("따라다닐 대상")] Transform target;

    Vector3 targetPos; // 카메라 위치
    bool cameraToggle = false;
    bool isFollowing = false;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraController에 Target이 없음");
            enabled = false;
            return;
        }

        targetPos = transform.position;
    }

    void LateUpdate()
    {
        CameraToggle();

        if (isFollowing)
        {
            CameraMove();
        }
        else
        {
            Vector3 dir = GetEdgeDirection();
            CameraDirMove(dir);
            ClampPosition();
        }

        transform.position = targetPos;
    }

    void CameraDirMove(Vector3 dir)
    {
        if (dir == Vector3.zero) return;

        targetPos += dir * moveSpeed * Time.deltaTime;
    }

    void ClampPosition()
    {
        targetPos.x = Mathf.Clamp(targetPos.x, minLimit.x, maxLimit.x);
        targetPos.z = Mathf.Clamp(targetPos.z, minLimit.y, maxLimit.y);
    }

    void CameraToggle()
    {
        if (GameInput.CameraFollowToggle.WasPressedThisFrame())
        {
            cameraToggle = !cameraToggle;
        }

        isFollowing = GameInput.CameraFollowHold.IsPressed() || cameraToggle;
    }

    void CameraMove()
    {
        targetPos = target.position;
    }

    Vector3 GetEdgeDirection()
    {
        Vector2 mouse = GameInput.PointerPosition;
        Vector3 dir = Vector3.zero;

        if (mouse.y >= Screen.height - borderSize) dir.z += 1; // 상
        if (mouse.y <= borderSize) dir.z -= 1; // 하
        if (mouse.x <= borderSize) dir.x -= 1; // 좌
        if (mouse.x >= Screen.width - borderSize) dir.x += 1; // 우
        return dir.normalized;// 대각선 이동 보간
    }
}
