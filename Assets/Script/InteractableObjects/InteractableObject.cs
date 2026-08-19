using DG.Tweening;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] Collider2D interactCollider;
    [SerializeField] private float dropDistance = 0.5f;
    [SerializeField] private float dropDuration = 0.3f;
    public Ease easeDrop = Ease.Linear;

    public void PickupBehaviour()
    {
        DeactivateCollider();
    }
    public void DropBehaviour()
    {
        DropAnimation();
        ActivateCollider(); // nanti buat setelah animasi atau bagaimana ntah lah
    }
    void DropAnimation()
    {
        transform.DOMoveY(transform.position.y - dropDistance, dropDuration).SetEase(easeDrop);
    }
    void DeactivateCollider()
    {
        interactCollider.enabled = false;
    }
    void ActivateCollider()
    {
        interactCollider.enabled = true;
    }
}
