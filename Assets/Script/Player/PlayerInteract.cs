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

    public bool isHoldingObject;

    InteractableObject currentHoldInteractScript;

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
        if(!IsPickUpValid())
        {
            Debug.LogError("ada target tapi posisi salah");
            return;
        }
        currentTargetObject.SetParent(holdTransform);
        currentTargetObject.position = holdTransform.position;
        currentHoldObject = currentTargetObject;

        currentHoldInteractScript = currentHoldObject.gameObject.GetComponent<InteractableObject>();
        currentHoldInteractScript.PickupBehaviour();

        isHoldingObject = false;
        DeselectTarget();
    }
    public void DropHoldObject()
    {
        if (currentHoldObject == null) return;

        currentHoldObject.SetParent(interactableParent);

        if (currentHoldInteractScript == null) return;
        currentHoldInteractScript.DropBehaviour();

        currentHoldInteractScript = null;
        currentHoldObject = null;
    }
    bool IsPickUpValid()
    {
        if (Vector3.Distance(transform.position, currentTargetObject.position) <= 0.11f) return true;
        return false;
    }
    public void FlipHoldTransform(bool isXPositif)
    {
        if (isXPositif) holdTransform.localPosition = new Vector3(0.5f, 0.5f, 0);
        else holdTransform.localPosition = new Vector3(-0.5f, 0.5f, 0);
    }
}
