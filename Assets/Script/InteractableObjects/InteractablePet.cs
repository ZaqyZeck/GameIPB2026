using DG.Tweening;
using UnityEngine;

public class InteractablePet : Interactables
{
    [SerializeField] private Pet ownerPet;
    [SerializeField] Collider2D interactCollider;
    [SerializeField] private float dropDistance = 0.5f;
    [SerializeField] private float dropDuration = 0.3f;
    [SerializeField] private Ease easeDrop = Ease.Linear;
    public bool isPickuped;

    public override void OnInteract(PlayerInteract player)
    {
        player.PickUpTargetObject();
    }

    public IHoldable GetHoldable() => ownerPet;
    public void PickupBehaviour()
    {
        isPickuped = true;
        DeactivateCollider();
    }
    public void DropBehaviour()
    {
        DropAnimation();
        //ActivateCollider(); // nanti buat setelah animasi atau bagaimana ntah lah
    }
    void DropAnimation()
    {
        transform.DOMoveY(transform.position.y - dropDistance, dropDuration).SetEase(easeDrop).OnComplete(() => { ActivateCollider(); isPickuped = false; });
    }
    void DeactivateCollider()
    {
        interactCollider.enabled = false;
    }
    void ActivateCollider()
    {
        interactCollider.enabled = true;
    }
    //private void OnDestroy()
    //{
    //    InteractableObject playerHolding = (InteractableObject) PlayerInteract.Instance.currentHoldObject.
    //    if (playerHolding == null) return;
    //    if (playerHolding == this) PlayerInteract.Instance.RemoveObjectyFromHold();
    //}

    //public override void Interact()
    //{
    //    //PickupBehaviour();
    //}
}
