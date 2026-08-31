using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class InteractablePet : Interactables
{
    [SerializeField] private Pet ownerPet;
    [SerializeField] Collider2D interactCollider;
    [SerializeField] private float dropDistance = 0.5f;
    [SerializeField] private float dropDuration = 0.3f;
    [SerializeField] private Ease easeDrop = Ease.Linear;
    //[SerializeField] private Transform interactableParent;
    public bool isPickuped;
    InteractableObject currentToy;
    public override void OnInteract(PlayerInteract player)
    {
        if (ownerPet.petData.hiddenAction != ActionTrait.Football && ownerPet.petData.hiddenAction != ActionTrait.CatToy)
        {
            player.PickUpTargetObject();
            return;
        }

        currentToy = player.GiveToy(this);

        if (currentToy == null)
        {
            player.PickUpTargetObject();
            return;
        }

        if (currentToy.actionTrait == ownerPet.petData.hiddenAction)
        {
            StartPlayToy();
            return;
        }

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

    void StartPlayToy()
    {

    }

    void StopPlayToy()
    {
        currentToy.transform.SetParent(MapManager.Instance.interactableParent);
        currentToy = null;
    }
    public Pet GetPet()
    {
        return ownerPet;
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
