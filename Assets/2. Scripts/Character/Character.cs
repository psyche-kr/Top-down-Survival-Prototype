using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(CharacterMovement))]
public class Character : MonoBehaviour
{
    public PlayerInputHandler InputHandler { get; private set; }
    public CharacterMovement Movement { get; private set; }

    IInteractable pendingInteract;
    float interactRange;
    Collider targetCollider;

    void Awake()
    {
        InputHandler = GetComponent<PlayerInputHandler>();
        Movement = GetComponent<CharacterMovement>();
    }

    void Update()
    {
        if (pendingInteract == null || targetCollider == null) return;

        if (IsInRange(pendingInteract))
        {
            InteractNow(pendingInteract);
        }
    }

    /// <summary>대상이 상호작용(습득) 범위 안인지.</summary>
    public bool IsInRange(IInteractable interactable)
    {
        float range = interactable.GetInteractRange();
        Vector3 closest = interactable.GetCollider().ClosestPoint(transform.position);
        return (transform.position - closest).sqrMagnitude <= range * range;
    }

    void InteractNow(IInteractable target)
    {
        CancelInteract();
        Movement.Stop();
        target.Interact(this);
    }

    /// <summary>지점으로 이동. 예약된 상호작용은 취소된다.</summary>
    public void MoveTo(Vector3 pos)
    {
        CancelInteract();
        Movement.MoveTo(pos);
    }

    /// <summary>정지. 예약된 상호작용은 취소된다.</summary>
    public void Stop()
    {
        CancelInteract();
        Movement.Stop();
    }

    public void MoveAndInteract(IInteractable interactable)
    {
        // 이미 범위 안이면 더 다가가지 않고 바로 상호작용
        if (IsInRange(interactable))
        {
            InteractNow(interactable);
            return;
        }

        pendingInteract = interactable;
        interactRange = interactable.GetInteractRange();
        targetCollider = interactable.GetCollider();

        Vector3 closest = targetCollider.ClosestPoint(transform.position);
        Movement.MoveTo(closest);
    }

    void CancelInteract()
    {
        pendingInteract = null;
        targetCollider = null;
    }
}
