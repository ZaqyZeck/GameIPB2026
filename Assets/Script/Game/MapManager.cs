using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    public Collider2D playerArea;
    public Collider2D PetArea;
    public Collider2D sunlightArea;
    public Collider2D doorArea;
    public Collider2D[] bedCollders;
    private void Awake()
    {
        Instance = this;
    }
    public Vector3 GetRandomPositionIn(Collider2D area)
    {
        Bounds bounds = area.bounds;

        for (int i = 0; i < 10; i++)
        {
            Vector2 randomPoint = new Vector2(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y));

            if (area.OverlapPoint(randomPoint))
            {
                return new Vector3(randomPoint.x, randomPoint.y, transform.position.z);
            }
        }

        return area.bounds.center;
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
