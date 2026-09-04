using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public static PlayerInteract Instance;
    [SerializeField] Transform holdTransform;
    [SerializeField] Transform interactableParent;

    public bool IsCurrentlyHover => currentHoverObject != null;
    public Transform currentHoverObject; // target object yang dibawah mouse
    public Transform currentTargetObject; // target object yang dikejar player
    public Transform currentHoldObject;
    public IHoldable CurrentHeldHoldable { get; private set; }

    public bool isHoldingObject;

    [SerializeField] private float pickUpRange = 2f;

    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        SelectHoverObject();

        bool inputLocked = PlayerMovement.Instance != null && PlayerMovement.Instance.IsMovementLocked;
        if (inputLocked) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlayerMovement.Instance.StopTargeting();
            if (IsCurrentlyHover) 
            {
                SelectTargetObject();
                PlayerMovement.Instance.ChangeTargetPosition(currentTargetObject.transform.position);
            }
            else
            {
                DeselectTarget();
                Vector3 mousePosition = Mouse.current.position.ReadValue();

                Vector3 screenPosition = new Vector3(mousePosition.x, mousePosition.y, Mathf.Abs(Camera.main.transform.position.z));

                Vector3 newTargetPosition = Camera.main.ScreenToWorldPoint(screenPosition);

                newTargetPosition.z = 0f;

                PlayerMovement.Instance.ChangeTargetPosition(newTargetPosition);
            }
        }
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            DropHoldObject();
        }
    }

    public void SelectHoverObject()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Vector3 screenPosition = new Vector3(mousePosition.x, mousePosition.y, Mathf.Abs(Camera.main.transform.position.z));
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;

        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition);

        currentHoverObject = null;

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("interactable"))
            {
                currentHoverObject = collider.transform;
                break;
            }
        }
    }
    public void SelectTargetObject()
    {
        if(currentHoverObject == null)
        {
            Debug.LogError("gak ada hover");
            return;
        }
        currentTargetObject = currentHoverObject;
        DeselectHover();
    }
    void DeselectTarget()
    {
        currentTargetObject = null;
    }
    void DeselectHover()
    {
        currentHoverObject = null;
    }

    public void PickUpTargetObject()
    {
        if (currentTargetObject == null)
        {
            Debug.LogError("gak ada target yang bisa di pick up");
            return;
        }
        if (!IsPickUpValid())
        {
            Debug.LogError("ada target tapi posisi salah");
            return;
        }
        if (isHoldingObject) DropHoldObject();

        IHoldable holdable = currentTargetObject.GetComponent<IHoldable>();

        currentTargetObject.SetParent(holdTransform);
        currentTargetObject.position = holdTransform.position;
        currentHoldObject = currentTargetObject;
        CurrentHeldHoldable = holdable;

        holdable.OnPickedUp(holdTransform);
        isHoldingObject = true;
        DeselectTarget();
    }
    public void DropHoldObject()
    {
        if (currentHoldObject == null) return;

        currentHoldObject.SetParent(interactableParent);
        CurrentHeldHoldable?.OnDropped(interactableParent);
        RemoveObjectFromHold();
    }

    
    bool IsPickUpValid()
    {
        if (Vector3.Distance(transform.position, currentTargetObject.position) <= pickUpRange) return true;
        return false;
    }
    public void FlipHoldTransform(bool isXPositif)
    {
        if (isXPositif) holdTransform.localPosition = new Vector3(0.5f, 0.5f, 0);
        else holdTransform.localPosition = new Vector3(-0.5f, 0.5f, 0);
    }

    public void InteractTarget()
    {
        if (currentTargetObject == null)
        {
            Debug.Log("Tidak ada target.");
            return;
        }
        Interactables interactable = currentTargetObject.GetComponent<Interactables>();
        if (interactable == null)
        {
            Debug.Log("Target bukan Interactable.");
            return;
        }
        interactable.OnInteract(this);
    }

    public void GivePet()
    {
        if (currentTargetObject == null || !isHoldingObject || CurrentHeldHoldable == null) return;

        Owner owner = currentTargetObject.GetComponent<Owner>();
        if (owner == null) return;

        if (owner.GetPet(CurrentHeldHoldable))
        {
            RemoveObjectFromHold();
        }
        DeselectTarget();
    }

    public InteractableObject GiveToy(InteractablePet pet)
    {
        if (!isHoldingObject || CurrentHeldHoldable == null || currentHoldObject == null) return null;
        if (pet == null) return null;

        InteractableObject toy = CurrentHeldHoldable as InteractableObject;
        if (toy == null) return null;
        if (toy.actionTrait != pet.GetPet().petData.hiddenAction) return null;

        currentHoldObject.SetParent(pet.transform);
        currentHoldObject.localPosition = Vector3.zero;
        RemoveObjectFromHold();

        return toy;
    }
    public void RemoveObjectFromHold()
    {
        CurrentHeldHoldable = null;
        currentHoldObject = null;
        isHoldingObject = false;
    }

    public Transform GetInteractableParent()
    {
        return interactableParent;
    }
}