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

        Vector3 closest = targetCollider.ClosestPoint(transform.position);

        float sqrDist = (transform.position - closest).sqrMagnitude;

        if (sqrDist <= interactRange * interactRange)
        {
            IInteractable target = pendingInteract;
            CancelInteract();
            Movement.Stop();
            target.Interact(this);
        }
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
