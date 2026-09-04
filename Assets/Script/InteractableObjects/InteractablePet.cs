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
        
        if (ownerPet.petData.hiddenAction != ActionTrait.Football && ownerPet.petData.hiddenAction != ActionTrait.CatToy)
        {
            player.PickUpTargetObject();
            return;
        }
        if (!player.isHoldingObject)
        {
            player.PickUpTargetObject();
            return;
        }
        //if (isPlaying)
        //{
        //    return;
        //}

        currentToy = player.GiveToy(this);

        if (currentToy == null || isPlaying)
        {
            //player.PickUpTargetObject();
            StopPlayToy();
            return;
        }

        if (currentToy.actionTrait == ownerPet.petData.hiddenAction)
        {
            StartPlayToy();
            return;
        }

        //player.PickUpTargetObject();
        StopPlayToy();
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
        if (!isPlaying) return;
        DropToy();
        isPlaying = false;
        //ownerPet.BehaviorController.TryStopAction(ownerPet.petData.hiddenAction);
    }
    public Pet GetPet()
    {
        return ownerPet;
    }
    
    void DropToy()
    {
        if (currentToy == null) return;
        Transform interactableParent = PlayerInteract.Instance.GetInteractableParent();
        //if (interactableParent == null)
        //{
        //    Debug.LogError(0);
        //    return;
        //}

        //Debug.LogError(1);
        currentToy.gameObject.transform.SetParent(interactableParent);
        currentToy.DropBehaviour();
        currentToy = null;
    }
}
