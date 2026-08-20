using DG.Tweening;
using UnityEngine;

public class InteractableObject : Interactable
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
        //ActivateCollider(); // nanti buat setelah animasi atau bagaimana ntah lah
    }
    void DropAnimation()
    {
        transform.DOMoveY(transform.position.y - dropDistance, dropDuration).SetEase(easeDrop).OnComplete(ActivateCollider);
    }
    void DeactivateCollider()
    {
        interactCollider.enabled = false;
    }
    void ActivateCollider()
    {
        interactCollider.enabled = true;
    }
    private void OnDestroy()
    {
        InteractableObject playerHolding = (InteractableObject) PlayerInteract.Instance.GetCurrentInteractScript();
        if (playerHolding == null) return;
        if (playerHolding == this) PlayerInteract.Instance.RemoveObjectyFromHold();
    }

    //public override void Interact()
    //{
    //    //PickupBehaviour();
    //}
}
