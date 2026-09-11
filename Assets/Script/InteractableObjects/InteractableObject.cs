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
    [SerializeField] private Material objectMaterial;
    //[SerializeField] private bool isShaderOutline;
    public bool isPickuped;
    public ActionTrait actionTrait;

    public Transform Transform => throw new System.NotImplementedException();

    private void Start()
    {
        if (objectMaterial != null) objectMaterial.SetFloat("_outlineOn", 0f);
    }

    public override void OnInteract(PlayerInteract player)
    {
        player.PickUpTargetObject();
    }

    public void PickupBehaviour()
    {
        isPickuped = true;
        DeactivateCollider();
        if (objectBehaviour != null) objectBehaviour.OnPickupBehaviour();
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

    public void SetVisible(bool isVisible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = isVisible;
    }

    void TurnOnOutline(bool isOn)
    {
        if (objectMaterial == null) return;
        if (isOn)
        {
            objectMaterial.SetFloat("_outlineOn", 1.0f);
        }
        else
        {
            //Debug.Log("dawdawwdwawda");
            objectMaterial.SetFloat("_outlineOn", 0f);
        }
    }

    public override void OnSelectedHover()
    {
        TurnOnOutline(true);
    }

    public override void OnDeselectedHover()
    {
        TurnOnOutline(false);
    }
}