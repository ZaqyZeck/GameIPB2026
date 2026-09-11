using System.Collections;
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
    [SerializeField] private float angryReactionDuration = 0.6f;
    //[SerializeField] private Material petMaterial;
    //[SerializeField] private Transform interactableParent;
    public bool isPickuped;
    bool isPlaying;
    InteractableObject currentToy;
    Coroutine angryReactionCoroutine;

    //private void Start()
    //{
    //    if (petMaterial != null) petMaterial.SetFloat("_outlineOn", 0f);
    //}
    public override void OnInteract(PlayerInteract player)
    {
        if (ownerPet.petData.hiddenAction != ActionTrait.Football &&
            ownerPet.petData.hiddenAction != ActionTrait.CatToy &&
            ownerPet.petData.hiddenAction != ActionTrait.MiceToy)
        {
            player.PickUpTargetObject();
            return;
        }
        if (!player.isHoldingObject)
        {
            player.PickUpTargetObject();
            return;
        }

        currentToy = player.GiveToy(this);

        if (currentToy == null || isPlaying)
        {
            StopPlayToy();
            return;
        }

        if (currentToy.actionTrait == ownerPet.petData.hiddenAction)
        {
            StartPlayToy();
            return;
        }

        PlayAngryReactionAndDropToy();
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

        ownerPet.Movement.Stop();

        currentToy?.SetVisible(false);
        ownerPet.BehaviorController.TryExecuteAction(ownerPet.petData.hiddenAction);
        PlayToySound();
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

        currentToy.SetVisible(true);
        currentToy.gameObject.transform.SetParent(interactableParent);
        currentToy.DropBehaviour();
        currentToy = null;
    }

    void PlayAngryReactionAndDropToy()
    {
        ownerPet.Movement.Stop();

        if (angryReactionCoroutine != null) StopCoroutine(angryReactionCoroutine);
        angryReactionCoroutine = StartCoroutine(AngryReactionRoutine());
        PlayAngrySound();
        DropToy();
    }

    IEnumerator AngryReactionRoutine()
    {
        float timer = angryReactionDuration;

        while (timer > 0f)
        {
            ownerPet.Animation.TriggerAction(PetAnimationIds.ReactionId_Angry);
            timer -= Time.deltaTime;
            yield return null;
        }

        ownerPet.Animation.ResetAction();
        angryReactionCoroutine = null;
    }

    //void GotAccepted()
    //{
    //    ownerPet.isAccepted = true;
    //}

    public override void OnSelectedHover()
    {
        TurnOnOutline(true);
    }

    public override void OnDeselectedHover()
    {
        TurnOnOutline(false);
    }

    void TurnOnOutline(bool isOn)
    {
        if (ownerPet.PetMaterial == null) return;
        if (isOn)
        {
            ownerPet.PetMaterial.SetFloat("_outlineOn", 1.0f);
        }
        else
        {
            ownerPet.PetMaterial.SetFloat("_outlineOn", 0f);
        }
    }
    void PlayAngrySound()
{
    if (GameManager.Instance == null) return;
    if (ownerPet == null || ownerPet.petData == null) return;

    if (ownerPet.petData.species == PetSpecies.Cat)
    {
        GameManager.Instance.PlayAudio(GameManager.Instance.meowAngry);
    }
    else if (ownerPet.petData.species == PetSpecies.Dog)
    {
        GameManager.Instance.PlayAudio(GameManager.Instance.dogAngry);
    }
    
}
void PlayToySound()
{
    if (GameManager.Instance == null) return;
    if (ownerPet == null || ownerPet.petData == null) return;

    if (ownerPet.petData.species == PetSpecies.Cat)
    {
        GameManager.Instance.PlayAudio(GameManager.Instance.meow);
    }
    else if (ownerPet.petData.species == PetSpecies.Dog)
    {
        GameManager.Instance.PlayAudio(GameManager.Instance.dog);
    }
}
}