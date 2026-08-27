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

    //Interactable currentHoldInteractScript;
    //Pet currentGhostPet;

    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        SelectHoverObject();

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
                Debug.Log("Hover: " + currentHoverObject.name);
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

        //InteractablePet interactableObj = currentTargetObject.GetComponent<InteractablePet>();
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
        RemoveObjectyFromHold();
    }

    public void RemoveObjectyFromHold()
    {
        CurrentHeldHoldable = null;
        currentHoldObject = null;
        isHoldingObject = false;
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
        Interactable interactable = currentTargetObject.GetComponent<Interactable>();
        if (interactable == null)
        {
            Debug.Log("Target bukan Interactable.");
            return;
        }
        interactable.OnInteract(this); // double dispatch — gak perlu tahu jenisnya apa
    }

    public void GivePet()
    {
        if (currentTargetObject == null || !isHoldingObject || CurrentHeldHoldable == null) return;

        Owner owner = currentTargetObject.GetComponent<Owner>();
        if (owner == null) return;

        if (owner.GetPet(CurrentHeldHoldable))
        {
            RemoveObjectyFromHold();
            //ReputationManager.Instance.Reward(100);
        }
        DeselectTarget();
    }

}
