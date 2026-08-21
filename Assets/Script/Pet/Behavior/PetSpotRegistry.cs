using UnityEngine;

public class PetSpotRegistry : MonoBehaviour
{
    public static PetSpotRegistry Instance;

    [SerializeField] private Transform doorSpot;
    [SerializeField] private Transform[] digSpots;

    private void Awake() => Instance = this;

    public Transform GetDoorSpot() => doorSpot;

    public Vector3 GetRandomDigSpot()
    {
        if (digSpots == null || digSpots.Length == 0) return transform.position;
        return digSpots[Random.Range(0, digSpots.Length)].position;
    }
}