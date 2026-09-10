using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pet : MonoBehaviour, IHoldable
{
    public int petId;
    public bool isOwnerArrived;
    public bool isEnteredDoor;
    //public bool isAccepted;
    public PetData petData;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] PetMovement movement;
    [SerializeField] PetBehaviorController behaviorController;
    [SerializeField] PetAnimation petAnimation;
    [SerializeField] InteractablePet petInteractable;

    [SerializeField] Vector3 targetSpawn;
    [SerializeField] TextMeshPro textPetId;
    [SerializeField] TextMeshPro textPetAction;
    [SerializeField] SpriteRenderer petRenderer;
    [SerializeField] Collider2D petCollider;
    public Material PetMaterial { get; private set; }
    public PetMovement Movement => movement;
    public PetAnimation Animation => petAnimation;
    public Transform Transform => transform;
    public PetBehaviorController BehaviorController => behaviorController;
    public void OnPickedUp(Transform holdPoint)
    {
        petInteractable.PickupBehaviour();
        //BehaviorController.TryStopAction(petData.hiddenAction);
        BehaviorController.PickUpBehaviour(petData.hiddenAction);
    }

    public void OnDropped(Transform dropParent)
    {
        petInteractable.DropBehaviour();
        BehaviorController.DropBehaviour();
    }

    public void SetFacing(bool isFacingPositiveX)
    {
        petAnimation.FlipSprite(isFacingPositiveX);
    }
    public void OwnerArrived()
    {
        isOwnerArrived = true;
    }

    public void SpawnAtDoor(PetData newPetData)
    {
        petData = newPetData;
        behaviorController.Initialize(petData);

        textPetId.text = petId.ToString();

        petAnimation.SetSpriteLibraryForType(petData.petType);

        //SpawnAnimation();
    }

    public void EnterDoor()
    {
        isEnteredDoor = true;
        SpawnAnimation();
    }

    public void Despawn()
    {
        DespawnAnimation();
    }
    void SpawnAnimation()
    {
        spriteRenderer.DOFade(1f, 1f).OnComplete(() =>
        {
            movement.MoveTo(MapManager.Instance.GetRandomPositionIn(MapManager.Instance.doorOpenArea));
        });
        petCollider.enabled = true;
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

    public void ChangeTextAction(string text)
    {
        textPetAction.text = text;
    }

    public void SetMaterial(Material material)
    {
        if (petRenderer != null) petRenderer.material = material;
        PetMaterial = material;
    }
}