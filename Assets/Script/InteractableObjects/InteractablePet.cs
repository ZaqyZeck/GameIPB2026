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
    bool isPlaying;
    InteractableObject currentToy;

    public override void OnInteract(PlayerInteract player)
    {
        if (isPlaying)
        {
            return;
        }
        if (ownerPet.petData.hiddenAction != ActionTrait.Football && ownerPet.petData.hiddenAction != ActionTrait.CatToy)
        {
            //player.PickUpTargetObject();
            return;
        }

        currentToy = player.GiveToy(this);

        if (currentToy == null)
        {
            //player.PickUpTargetObject();
            DropToy();
            return;
        }

        if (currentToy.actionTrait == ownerPet.petData.hiddenAction)
        {
            StartPlayToy();
            return;
        }

        //player.PickUpTargetObject();
        DropToy();
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
        isPlaying = true;
        ownerPet.BehaviorController.TryExecuteAction(ownerPet.petData.hiddenAction);
    }

    public void StopPlayToy()
    {
        DropToy();
        isPlaying = false;
    }
    public Pet GetPet()
    {
        return ownerPet;
    }
    
    void DropToy()
    {
        if (currentToy == null) return;
        currentToy.Transform.SetParent(PlayerInteract.Instance.GetInteractableParent());
        currentToy.DropBehaviour();
        currentToy = null;
    }
}
