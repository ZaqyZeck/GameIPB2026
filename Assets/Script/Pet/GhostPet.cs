using UnityEngine;

public class GhostPet : MonoBehaviour
{
    public int petId;
    public bool isOwnerArrived;

    public void OwnerArrived()
    {
        isOwnerArrived = true;
    }
}
