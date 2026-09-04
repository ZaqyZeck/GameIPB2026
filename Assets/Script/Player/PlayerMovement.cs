using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    [SerializeField] PlayerInteract playerInteract;
    [SerializeField] PolygonCollider2D playerArea;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Seeker seeker;

    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float reachDistance = 0.1f;
    [SerializeField] private bool haveTarget;
    [SerializeField] private bool isMovementLocked;

    private Vector3 targetPosition;
    private List<Vector3> path;
    private int currentWaypoint;

    private Transform TargetObject => PlayerInteract.Instance.currentTargetObject;

    public bool IsMovementLocked => isMovementLocked;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (haveTarget && !isMovementLocked) GoToTarget();
    }

    public void ChangeTargetPosition(Vector3 target)
    {
        if (isMovementLocked) return;

        targetPosition = GetValidTargetPosition(target);

        seeker.StartPath(transform.position, targetPosition, OnPathComplete);

        bool isXPositif = targetPosition.x - transform.position.x > 0f;
        HandleFlip(isXPositif);

        haveTarget = true;
    }

    public void StopTargeting()
    {
        haveTarget = false;
        path = null;
        currentWaypoint = 0;
    }

    /// <summary>
    /// Locks or unlocks player movement (e.g. while a dialogue is open).
    /// Locking immediately cancels any in-progress path.
    /// </summary>
    public void SetMovementLocked(bool locked)
    {
        isMovementLocked = locked;
        if (locked)
        {
            StopTargeting();
        }
    }

    private void OnPathComplete(Path newPath)
    {
        if (newPath.error)
        {
            Debug.LogWarning($"Pathfinding failed: {newPath.errorLog}");
            StopTargeting();
            return;
        }

        path = newPath.vectorPath;
        currentWaypoint = 0;
    }

    private void GoToTarget()
    {
        if (path == null || path.Count == 0) return;

        if (currentWaypoint >= path.Count)
        {
            ReachedTarget();
            return;
        }

        Vector3 waypoint = path[currentWaypoint];
        waypoint.z = transform.position.z;

        Vector3 direction = waypoint - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            bool isXPositif = direction.x > 0f;
            HandleFlip(isXPositif);
        }

        transform.position = Vector3.MoveTowards(transform.position, waypoint, movementSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, waypoint) <= reachDistance)
        {
            currentWaypoint++;
        }
    }

    private void ReachedTarget()
    {
        if (TargetObject != null)
        {
            PlayerInteract.Instance.InteractTarget();
        }

        StopTargeting();
    }

    private bool CanMoveTo(Vector3 target)
    {
        return playerArea.OverlapPoint(target);
    }

    private Vector3 GetValidTargetPosition(Vector3 target)
    {
        if (CanMoveTo(target)) return target;

        Vector2 closestPoint = playerArea.ClosestPoint(target);
        Vector2 direction = ((Vector2)target - closestPoint).normalized;
        Vector2 validPoint = closestPoint - direction * 0.01f;

        return new Vector3(validPoint.x, validPoint.y, transform.position.z);
    }

    private void HandleFlip(bool isXPositif)
    {
        if (spriteRenderer.flipX == isXPositif) return;

        playerInteract.CurrentHeldHoldable?.SetFacing(isXPositif);
        spriteRenderer.flipX = isXPositif;
        PlayerInteract.Instance.FlipHoldTransform(isXPositif);
    }
}