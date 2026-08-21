using UnityEngine;

public class PetMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float stoppingDistance = 0.1f;

    private Vector3? destination;
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
        if (!destination.HasValue)
        {
            IsMoving = false;
            return;
        }

        Vector3 target = destination.Value;

        if (Vector3.Distance(transform.position, target) <= stoppingDistance)
        {
            destination = null;
            IsMoving = false;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        IsMoving = true;
    }
}