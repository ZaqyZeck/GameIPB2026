using UnityEngine;

public abstract class ObjectBehaviour : MonoBehaviour
{
    [SerializeField] protected InteractableObject interactableObject;
    public ActionTrait trait;

    public abstract void OnFloorBehaviour();
    public abstract void OnPickupBehaviour();
    public abstract void OnDropBehaviour();
}
