using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;
    [SerializeField] PolygonCollider2D barrierCollider;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private bool haveTarget;

    private Vector3 targetPosition;
    private Transform TargetObject => PlayerInteract.Instance.currentTargetObject;

    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        if (haveTarget) GoToTarget();
    }

    public void ChangeTargetPosition(Vector3 target)
    {
        targetPosition = GetValidTargetPosition(target);
        bool isXPositif = targetPosition.x - transform.position.x > 0f;
        HandleFlip(isXPositif);
        haveTarget = true;
    }
    public void StopTargeting()
    {
        haveTarget = false;
    }
    private void GoToTarget()
    {
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
        if(Vector3.Distance(transform.position, targetPosition) <= 0.1)
        {
            ReachedTarget();
            return;
        }

        transform.position = nextPosition;
    }
    private void ReachedTarget()
    {
        if(TargetObject != null)
        {
            PlayerInteract.Instance.PickUpTargetObject();
        }
        StopTargeting();
    }
    private bool CanMoveTo(Vector3 target)
    {
        return barrierCollider.OverlapPoint(target);
    }

    private Vector3 GetValidTargetPosition(Vector3 target)
    {
        if (CanMoveTo(target)) return target;

        Vector2 closestPoint = barrierCollider.ClosestPoint(target);

        Vector2 direction = ((Vector2)target - closestPoint).normalized;

        Vector2 validPoint = closestPoint - direction * 0.01f;

        return new Vector3(validPoint.x, validPoint.y, transform.position.z);
    }

    private void HandleFlip(bool isXPositif)
    {
        if (spriteRenderer.flipX == isXPositif) return;

        spriteRenderer.flipX = isXPositif;
        PlayerInteract.Instance.FlipHoldTransform(isXPositif);
    }
}
