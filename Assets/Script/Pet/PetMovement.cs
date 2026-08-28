using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PetMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float stoppingDistance = 0.1f;
    [SerializeField] private InteractablePet petInteractable;
    [SerializeField] private PetAnimation petAnimation;

    [SerializeField] private bool usePathfinding;
    [SerializeField] private Seeker seeker;

    private Vector3? destination;
    private Coroutine wanderCoroutine;

    private List<Vector3> path;
    private int currentWaypoint;

    public bool IsMoving { get; private set; }

    public void MoveTo(Vector3 target)
    {
        destination = target;

        if (usePathfinding)
        {
            seeker.StartPath(transform.position, target, OnPathComplete);
        }
        else
        {
            path = null;
            currentWaypoint = 0;
        }
    }

    public void Stop()
    {
        destination = null;
        path = null;
        currentWaypoint = 0;
        IsMoving = false;
    }

    private void Update()
    {
        if (!destination.HasValue || petInteractable.isPickuped)
        {
            IsMoving = false;
            return;
        }

        if (usePathfinding)
        {
            MoveWithPathfinding();
        }
        else
        {
            MoveDirectly();
        }
    }

    private void MoveDirectly()
    {
        Vector3 target = destination.Value;

        if (Vector3.Distance(transform.position, target) <= stoppingDistance)
        {
            ReachedDestination();
            return;
        }

        petAnimation.FlipSprite(transform.position.x - target.x < 0);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        IsMoving = true;
    }

    private void MoveWithPathfinding()
    {
        if (path == null || path.Count == 0)
        {
            IsMoving = false;
            return;
        }

        if (currentWaypoint >= path.Count)
        {
            ReachedDestination();
            return;
        }

        Vector3 waypoint = path[currentWaypoint];
        waypoint.z = transform.position.z;

        if (Vector3.Distance(transform.position, waypoint) <= stoppingDistance)
        {
            currentWaypoint++;

            if (currentWaypoint >= path.Count)
            {
                ReachedDestination();
                return;
            }

            waypoint = path[currentWaypoint];
            waypoint.z = transform.position.z;
        }

        petAnimation.FlipSprite(transform.position.x - waypoint.x < 0);

        transform.position = Vector3.MoveTowards(transform.position, waypoint, moveSpeed * Time.deltaTime);
        IsMoving = true;
    }

    private void OnPathComplete(Path newPath)
    {
        if (newPath.error)
        {
            Debug.LogWarning($"Pet pathfinding failed: {newPath.errorLog}");
            path = null;
            IsMoving = false;
            return;
        }

        path = newPath.vectorPath;
        currentWaypoint = 0;
    }

    private void ReachedDestination()
    {
        destination = null;
        path = null;
        currentWaypoint = 0;
        IsMoving = false;

        WanderAround();
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
