using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 스페이스바를 누르고 있는 동안 내 캐릭터 위치를 알려 주는 표시.
/// 발밑에 노란 원(LineRenderer)을 그리고, 머리 위 UI의 V자 표시를 켠다.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class PlayerLocator : MonoBehaviour
{
    [Tooltip("머리 위 UI를 만드는 프로필 (비워두면 부모에서 찾음)")]
    [SerializeField] UnitProfile profile;
    [SerializeField] float radius = 1.1f;
    [SerializeField] float lineWidth = 0.1f;
    [SerializeField] int segments = 64;
    [SerializeField] Color ringColor = new Color(1f, 0.82f, 0.4f, 0.9f);
    [Tooltip("바닥과 겹쳐 깜빡이지 않도록 살짝 띄우는 높이")]
    [SerializeField] float groundOffset = 0.03f;

    LineRenderer ring;
    NavMeshAgent agent;
    Transform owner;

    void Awake()
    {
        ring = GetComponent<LineRenderer>();
        owner = transform.parent != null ? transform.parent : transform;
        agent = owner.GetComponent<NavMeshAgent>();
        if (profile == null) profile = owner.GetComponent<UnitProfile>();

        ring.useWorldSpace = true;
        ring.loop = true;
        ring.positionCount = segments;
        ring.widthMultiplier = lineWidth;
        ring.startColor = ring.endColor = ringColor;
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows = false;
        ring.enabled = false;
    }

    void LateUpdate()
    {
        bool show = GameInput.CameraFollowHold.IsPressed();

        ring.enabled = show;
        CharacterOverheadUI overheadUI = profile != null ? profile.OverheadUI : null;
        if (overheadUI != null && overheadUI.LocatorMarker != null)
            overheadUI.LocatorMarker.SetActive(show);

        if (show) UpdateRing();
    }

    void UpdateRing()
    {
        // NavMeshAgent 기준점은 발에서 baseOffset만큼 위에 있다
        float groundY = owner.position.y - (agent != null ? agent.baseOffset : 0f) + groundOffset;
        Vector3 center = new Vector3(owner.position.x, groundY, owner.position.z);

        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2f / segments;
            ring.SetPosition(i, center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius);
        }
    }
}
