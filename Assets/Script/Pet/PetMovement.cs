using System.Collections;
using UnityEngine;

public class PetMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float stoppingDistance = 0.1f;
    [SerializeField] private InteractableObject petInteractable;
    private Vector3? destination;
    private Coroutine wanderCoroutine;
    public bool IsMoving { get; private set; }

    public void MoveTo(Vector3 target)
    {
        destination = target;
    }

    public void Stop()
    {
        destination = null;
        IsMoving = false;
    }

    private void Update()
    {
        if (!destination.HasValue || petInteractable.isPickuped)
        {
            IsMoving = false;
            return;
        }

        Vector3 target = destination.Value;

        if (Vector3.Distance(transform.position, target) <= stoppingDistance)
        {
            destination = null;
            IsMoving = false;
            WanderAround();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        IsMoving = true;
    }

    private void WanderAround()
    {
        if (wanderCoroutine != null) return;

        wanderCoroutine = StartCoroutine(WanderDelay());
    }

    private IEnumerator WanderDelay()
    {
        yield return new WaitForSeconds(3f);

        wanderCoroutine = null;
        MoveTo(PetManager.Instance.GetRandomPosition());
    }
}