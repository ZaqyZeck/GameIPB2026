using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Action Icon")]
    [SerializeField] private Image actionIconImage;
    [SerializeField] private float actionIconDuration = 5f;
    private Coroutine actionIconCoroutine;

    [Header("Cat Toy Prop")]
    [SerializeField] private Transform catToyPropTransform;
    [SerializeField] private float catToyWaveAngle = 20f;
    [SerializeField] private float catToyWaveDuration = 0.3f;
    private Tween catToyWaveTween;
    private Vector3 catToyPropDefaultLocalPos;
    private Vector3 catToyPropDefaultLocalScale;
    private bool catToyPropDefaultsCaptured;

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

        petAnimation.SetWalking(false);
        petAnimation.SetSitting(false);
        petAnimation.ResetAction();
    }

    public void OnDropped(Transform dropParent)
    {
        petInteractable.DropBehaviour();
        BehaviorController.DropBehaviour();

        petAnimation.SetWalking(false);
        petAnimation.SetSitting(false);
        petAnimation.ResetAction();
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

    public void ShowActionIcon(ActionTrait trait)
    {
        if (actionIconImage == null) return;

        Sprite icon = PetIconDatabase.Instance != null ? PetIconDatabase.Instance.GetActionIcon(trait) : null;

        if (icon == null) return;

        if (actionIconCoroutine != null) StopCoroutine(actionIconCoroutine);

        actionIconImage.sprite = icon;
        actionIconImage.gameObject.SetActive(true);

        actionIconCoroutine = StartCoroutine(HideActionIconAfterDelay(actionIconDuration));
    }

    public void HideActionIcon()
    {
        if (actionIconCoroutine != null)
        {
            StopCoroutine(actionIconCoroutine);
            actionIconCoroutine = null;
        }

        if (actionIconImage != null) actionIconImage.gameObject.SetActive(false);
    }

    private IEnumerator HideActionIconAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        actionIconImage.gameObject.SetActive(false);
        actionIconCoroutine = null;
    }

    public void ShowCatToyProp()
    {
        if (catToyPropTransform == null) return;

        CaptureCatToyPropDefaultsIfNeeded();

        catToyWaveTween?.Kill();

        // Mirror position and sprite depending on facing direction.
        // catToyPropDefaultLocalPos/Scale represent the "facing left" (default) setup.
        bool facingRight = petAnimation.IsFacingPositiveX;

        if (facingRight)
        {
            catToyPropTransform.localPosition = new Vector3(
                -catToyPropDefaultLocalPos.x,
                catToyPropDefaultLocalPos.y,
                catToyPropDefaultLocalPos.z);

            catToyPropTransform.localScale = new Vector3(
                -catToyPropDefaultLocalScale.x,
                catToyPropDefaultLocalScale.y,
                catToyPropDefaultLocalScale.z);
        }
        else
        {
            catToyPropTransform.localPosition = catToyPropDefaultLocalPos;
            catToyPropTransform.localScale = catToyPropDefaultLocalScale;
        }

        catToyPropTransform.gameObject.SetActive(true);
        catToyPropTransform.localRotation = Quaternion.identity;

        catToyWaveTween = catToyPropTransform
            .DORotate(new Vector3(0f, 0f, catToyWaveAngle), catToyWaveDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void HideCatToyProp()
    {
        catToyWaveTween?.Kill();
        catToyWaveTween = null;

        if (catToyPropTransform != null)
        {
            catToyPropTransform.localRotation = Quaternion.identity;
            catToyPropTransform.gameObject.SetActive(false);
        }
    }

    private void CaptureCatToyPropDefaultsIfNeeded()
    {
        if (catToyPropDefaultsCaptured) return;

        catToyPropDefaultLocalPos = catToyPropTransform.localPosition;
        catToyPropDefaultLocalScale = catToyPropTransform.localScale;
        catToyPropDefaultsCaptured = true;
    }
}