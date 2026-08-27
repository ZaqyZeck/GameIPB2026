using DG.Tweening;
using TMPro;
using UnityEngine;

public class Pet : MonoBehaviour, IHoldable
{
    public int petId;
    public bool isOwnerArrived;
    public PetData petData;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] PetMovement movement;
    [SerializeField] PetBehaviorController behaviorController;
    [SerializeField] PetAnimation petAnimation;
    [SerializeField] InteractablePet petInteractable;

    [SerializeField] Vector3 targetSpawn;
    [SerializeField] TextMeshPro textPetId;

    public PetMovement Movement => movement;
    public Transform Transform => transform;
    public PetBehaviorController BehaviorController => behaviorController;
    public void OnPickedUp(Transform holdPoint)
    {
        petInteractable.PickupBehaviour();
    }

    public void OnDropped(Transform dropParent)
    {
        petInteractable.DropBehaviour();
    }

    public void SetFacing(bool isFacingPositiveX)
    {
        petAnimation.FlipSprite(isFacingPositiveX);
    }
    public void OwnerArrived()
    {
        isOwnerArrived = true;
    }

    public void Spawn(PetData newPetData)
    {
        petData = newPetData;
        behaviorController.Initialize(petData);

        textPetId.text = petId.ToString();

        SpawnAnimation();
    }

    public void Despawn()
    {
        DespawnAnimation();
    }
    void SpawnAnimation()
    {
        spriteRenderer.DOFade(1f, 1f).OnComplete(()=> { movement.MoveTo(targetSpawn); });
    }

    void DespawnAnimation()
    {
        spriteRenderer.DOFade(0f, 1f).OnComplete(() => DestroySelf());
        //DestroySelf();
    }
    void DestroySelf()
    {
        Destroy(gameObject);
    }

    public InteractablePet GetInteractable()
    {
        return petInteractable;
    }

    public PetAnimation GetPetAnimation()
    {
        return petAnimation;
    }
}