using System;
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

    //[SerializeField] private bool usePathfinding;
    [SerializeField] private Seeker seeker;

    private Vector3? destination;
    private Action onArrived;
    private bool wanderAfterArrival;
    private Coroutine wanderCoroutine;

    private List<Vector3> path;
    private int currentWaypoint;

    public bool IsMoving { get; private set; }

    public void MoveTo(Vector3 target)
    {
        CancelWander();

        SetDestination(target, onArrived: null, wanderAfterArrival: true);
        seeker.StartPath(transform.position, target, OnPathComplete);
    }

    public void MoveTo(Vector3 target, Action onArrivedCallback)
    {
        CancelWander();

        SetDestination(target, onArrived: onArrivedCallback, wanderAfterArrival: false);
        seeker.StartPath(transform.position, target, OnPathComplete);
    }

    private void SetDestination(Vector3 target, Action onArrived, bool wanderAfterArrival)
    {
        destination = target;
        this.onArrived = onArrived;
        this.wanderAfterArrival = wanderAfterArrival;
    }

    public void Stop()
    {
        CancelWander();

        destination = null;
        path = null;
        currentWaypoint = 0;
        IsMoving = false;
        onArrived = null;
        wanderAfterArrival = true;
        petAnimation.SetWalking(false);
    }

    private void Update()
    {
        if (!destination.HasValue || petInteractable.isPickuped)
        {
            IsMoving = false;
            petAnimation.SetWalking(false);
            return;
        }

        Move();

    }

    private void Move()
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
        petAnimation.SetWalking(true);
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
        petAnimation.SetWalking(false);
        PlayArriveCallBack();
    }

    public void WanderAround()
    {
        if (wanderCoroutine != null) return;

        wanderCoroutine = StartCoroutine(WanderDelay());
    }
    private void PlayArriveCallBack()
    {
        IsMoving = false;

        Action callback = onArrived;
        onArrived = null;

        if (callback != null)
        {
            callback.Invoke();
        }
        else if (wanderAfterArrival)
        {
            WanderAround();
        }
    }

    private void CancelWander()
    {
        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
            wanderCoroutine = null;
        }
    }

    private IEnumerator WanderDelay()
    {
        yield return new WaitForSeconds(3f);

        wanderCoroutine = null;
        MoveTo(PetManager.Instance.GetRandomPosition());
    }

    public bool IsNear(Vector3 position,  float distance)
    {
        return Vector3.Distance(position, transform.position) < distance;
    }

    public Vector3 GetPositionNear(Vector3 position, Vector3 targetPosition)
    {
        Vector3 direction = (position - targetPosition).normalized;
        float distance = UnityEngine.Random.Range(0.3f, .7f);
        float yOffset = UnityEngine.Random.Range(0f, 0.5f);

        Vector3 result = targetPosition + direction * distance;
        result.y += yOffset;

        return result;
    }
}