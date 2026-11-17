using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 유닛의 이동 속도를 자식 모델의 Animator에 전달하고, 발 움직임이 실제 이동 속도와 맞도록 맞춘다.
///
/// 이동은 NavMeshAgent가 하므로 애니메이션은 제자리(InPlace) 클립을 쓰고 루트 모션은 끈다.
/// 제자리 클립 자체에는 속도 정보가 없어서, 같은 동작의 루트 모션 클립에서 잰 속도(휴머노이드 정규화 값)를
/// 모델 크기(humanScale × 모델 스케일 ≈ 골반 높이)에 곱해 "이 모델이 이 클립을 1배속으로 재생할 때의 실제 속도"를 구한다.
///
/// 블렌드 파라미터 Gait: 0 대기, 1 걷기, 2 달리기 (CharacterLocomotion 컨트롤러)
///   - 걷기 속도 이하: 대기와 걷기를 섞음 (재생 속도 1)
///   - 걷기~달리기 사이: 두 클립의 자연 속도 비율로 섞음
///   - 달리기 속도 이상: 달리기를 빠르게 재생
///   alwaysRun이면 움직이는 동안 항상 달리기 동작을 쓰고 재생 속도로 맞춘다 (이터널 리턴처럼 걷기 없이 뛰는 느낌).
/// 모델은 유닛의 자식으로 두기만 하면 되어, 다른 휴머노이드 모델로 바꿔도 속도가 자동으로 다시 맞춰진다.
/// </summary>
public class CharacterAnimator : MonoBehaviour
{
    static readonly int GaitId = Animator.StringToHash("Gait");

    [Tooltip("비워두면 자식에서 찾음")]
    [SerializeField] Animator animator;

    [Header("클립 속도 (루트 모션 클립의 averageSpeed, 휴머노이드 정규화 값)")]
    [Tooltip("DoubleL OneHand_Up_Walk_F (1.967s)")]
    [SerializeField] float walkClipSpeed = 1.96f;
    [Tooltip("DoubleL OneHand_Up_Run_F (1.167s)")]
    [SerializeField] float runClipSpeed = 4.60f;

    [Header("동작")]
    [Tooltip("움직이는 동안 항상 달리기 동작을 쓰고, 재생 속도로 이동 속도에 맞춘다 (이터널 리턴 방식)")]
    [SerializeField] bool alwaysRun = true;
    [Tooltip("재생 속도 배율 한계 (너무 느리거나 빠른 재생 방지)")]
    [SerializeField] Vector2 playbackRange = new Vector2(0.3f, 2f);
    [Tooltip("이 속도 미만은 멈춘 것으로 봄")]
    [SerializeField] float stopThreshold = 0.05f;
    [Tooltip("블렌드 값이 바뀔 때 부드럽게 따라가는 시간")]
    [SerializeField] float gaitDamping = 0.1f;

    NavMeshAgent agent;
    float walkSpeed, runSpeed; // 이 모델 기준 실제 속도 (m/s)

    /// <summary>이 모델이 걷기 클립을 1배속으로 재생할 때의 이동 속도.</summary>
    public float NaturalWalkSpeed => walkSpeed;
    /// <summary>이 모델이 달리기 클립을 1배속으로 재생할 때의 이동 속도.</summary>
    public float NaturalRunSpeed => runSpeed;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false;
        RecalculateNaturalSpeeds();
    }

    /// <summary>모델을 바꾸거나 크기를 바꾼 뒤 호출하면 자연 속도를 다시 계산한다.</summary>
    public void RecalculateNaturalSpeeds()
    {
        if (animator == null) return;

        // humanScale은 아바타 기준 골반 높이 비율, 여기에 모델 스케일을 곱하면 월드 기준 크기
        float bodyScale = animator.isHuman ? animator.humanScale * animator.transform.lossyScale.y : 1f;
        walkSpeed = Mathf.Max(0.01f, walkClipSpeed * bodyScale);
        runSpeed = Mathf.Max(walkSpeed + 0.01f, runClipSpeed * bodyScale);
    }

    void Update()
    {
        if (animator == null) return;

        float speed = agent != null && agent.enabled ? agent.velocity.magnitude : 0f;

        float gait;
        float playback = 1f;

        if (speed < stopThreshold)
        {
            gait = 0f;
        }
        else if (alwaysRun)
        {
            gait = 2f;
            playback = speed / runSpeed;
        }
        else if (speed <= walkSpeed)
        {
            // 대기↔걷기 블렌드: 걷기 비중만큼 보폭이 줄어 속도가 맞는다
            gait = speed / walkSpeed;
        }
        else if (speed <= runSpeed)
        {
            // 걷기↔달리기 블렌드: 섞인 동작의 자연 속도 ≈ 두 속도의 보간
            gait = 1f + (speed - walkSpeed) / (runSpeed - walkSpeed);
        }
        else
        {
            gait = 2f;
            playback = speed / runSpeed;
        }

        animator.SetFloat(GaitId, gait, gaitDamping, Time.deltaTime);
        animator.speed = Mathf.Clamp(playback, playbackRange.x, playbackRange.y);
    }
}
