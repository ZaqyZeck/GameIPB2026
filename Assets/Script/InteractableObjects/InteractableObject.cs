using DG.Tweening;
using UnityEngine;

public class InteractableObject : Interactables, IHoldable
{
    //[SerializeField] private Pet ownerPet;
    [SerializeField] ObjectBehaviour objectBehaviour;
    [SerializeField] Collider2D interactCollider;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] private float dropDistance = 0.5f;
    [SerializeField] private float dropDuration = 0.3f;
    [SerializeField] private Ease easeDrop = Ease.Linear;
    public bool isPickuped;

    public Transform Transform => throw new System.NotImplementedException();

    public override void OnInteract(PlayerInteract player)
    {
        player.PickUpTargetObject();
    }

    //public IHoldable GetHoldable() => ownerPet;
    public void PickupBehaviour()
    {
        isPickuped = true;
        DeactivateCollider();
        objectBehaviour.OnPickupBehaviour();
    }
    public void DropBehaviour()
    {
        DropAnimation();
        objectBehaviour.OnDropBehaviour();
        //ActivateCollider(); // nanti buat setelah animasi atau bagaimana ntah lah
    }
    void DropAnimation()
    {
        transform.DOMoveY(transform.position.y - dropDistance, dropDuration).SetEase(easeDrop).OnComplete(() => 
            { 
                ActivateCollider(); 
                isPickuped = false; 
                objectBehaviour.OnFloorBehaviour(); 
            });
    }
    void DeactivateCollider()
    {
        interactCollider.enabled = false;
    }
    void ActivateCollider()
    {
        interactCollider.enabled = true;
    }

    public void OnPickedUp(Transform holdPoint)
    {
        PickupBehaviour();
    }

    public void OnDropped(Transform dropParent)
    {
        DropBehaviour();
    }

    public void SetFacing(bool isFacingPositiveX)
    {
        spriteRenderer.flipX = isFacingPositiveX;
    }
}
