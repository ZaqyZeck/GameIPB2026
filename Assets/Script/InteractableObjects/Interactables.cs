using UnityEngine;

public abstract class Interactables : MonoBehaviour
{
    //public abstract void Interact();
    public abstract void OnInteract(PlayerInteract player);
    public abstract void OnSelectedHover();
    public abstract void OnDeselectedHover();


}
