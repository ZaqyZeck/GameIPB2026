using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public static PlayerInteract Instance;
    [SerializeField] Transform pickUpTransform;
    public bool IsCurrentlyHover => currentHoverObject != null;
    public GameObject currentHoverObject; // target object yang dibawah mouse
    public GameObject currentTargetObject; // target object yang dikejar player

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
                currentHoverObject = collider.gameObject;
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
        currentTargetObject.transform.SetParent(pickUpTransform);
        currentTargetObject.transform.position = pickUpTransform.position;
        DeselectTarget();
    }
    bool IsPickUpValid()
    {
        if (Vector3.Distance(transform.position, currentTargetObject.transform.position) <= 0.11f) return true;
        return false;
    }
}
