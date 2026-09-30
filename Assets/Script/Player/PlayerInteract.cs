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

    [Header("Double Tap / Click Drop Settings")]
    [Tooltip("Maximum time (in seconds) between two taps/clicks to count as a double-click drop.")]
    [SerializeField] private float doubleClickThreshold = 0.35f;
    [Tooltip("Maximum screen pixel distance between two taps/clicks to count as a double-click drop.")]
    [SerializeField] private float doubleClickMaxDistance = 100f;

    private float lastClickTime = -1f;
    private Vector2 lastClickPosition;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnPrimaryTap += HandlePrimaryTap;
            GameInputManager.Instance.OnDrop += DropHoldObject;
        }
    }

    private void Start()
    {
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnPrimaryTap -= HandlePrimaryTap;
            GameInputManager.Instance.OnPrimaryTap += HandlePrimaryTap;
            GameInputManager.Instance.OnDrop -= DropHoldObject;
            GameInputManager.Instance.OnDrop += DropHoldObject;
        }
    }

    private void OnDisable()
    {
        lastClickTime = -1f;
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnPrimaryTap -= HandlePrimaryTap;
            GameInputManager.Instance.OnDrop -= DropHoldObject;
        }
    }

    private void Update()
    {
        if (!LevelManager.Instance.IsPlaying) return;

        if (GameInputManager.Instance != null)
        {
            if (GameInputManager.Instance.IsPointerOverUI())
            {
                DeselectHover();
                return;
            }
            SelectHoverObject(GameInputManager.Instance.CurrentPointerPosition);
        }
    }

    private void HandlePrimaryTap(Vector2 screenPosition)
    {
        if (LevelManager.Instance != null && !LevelManager.Instance.IsPlaying) return;
        if (PlayerMovement.Instance != null && PlayerMovement.Instance.IsMovementLocked) return;
        if (GameInputManager.Instance != null && GameInputManager.Instance.IsPointerOverUI()) return;

        // Double-click / double-tap detection for dropping held object
        if (isHoldingObject)
        {
            float timeSinceLastClick = Time.time - lastClickTime;
            float dist = Vector2.Distance(screenPosition, lastClickPosition);

            if (lastClickTime > 0f && timeSinceLastClick <= doubleClickThreshold && dist <= doubleClickMaxDistance)
            {
                lastClickTime = -1f;
                if (PlayerMovement.Instance != null)
                {
                    PlayerMovement.Instance.StopTargeting();
                }
                DeselectTarget();
                DropHoldObject();
                return;
            }

            lastClickTime = Time.time;
            lastClickPosition = screenPosition;
        }
        else
        {
            lastClickTime = -1f;
        }

        PlayerMovement.Instance.StopTargeting();

        Transform hitInteractable = GetInteractableAtScreenPosition(screenPosition);
        if (hitInteractable != null)
        {
            currentHoverObject = hitInteractable;
            SelectTargetObject();
            PlayerMovement.Instance.ChangeTargetPosition(currentTargetObject.transform.position);
        }
        else
        {
            DeselectTarget();
            if (Camera.main != null)
            {
                Vector3 screenPos3D = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z));
                Vector3 newTargetPosition = Camera.main.ScreenToWorldPoint(screenPos3D);
                newTargetPosition.z = 0f;
                PlayerMovement.Instance.ChangeTargetPosition(newTargetPosition);
            }
        }
    }

    public void SelectHoverObject(Vector2 screenPosition)
    {
        Transform hit = GetInteractableAtScreenPosition(screenPosition);
        if (hit == currentHoverObject) return;

        DeselectHover();
        currentHoverObject = hit;

        if (currentHoverObject != null)
        {
            Interactables interactableObject = currentHoverObject.GetComponent<Interactables>();
            if (interactableObject != null)
            {
                interactableObject.OnSelectedHover();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayAudio(GameManager.Instance.hover);
                }
            }
        }
    }

    public Transform GetInteractableAtScreenPosition(Vector2 screenPosition)
    {
        if (Camera.main == null) return null;

        Vector3 screenPos3D = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z));
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPos3D);
        worldPosition.z = 0f;

        Collider2D[] colliders = Physics2D.OverlapPointAll(worldPosition);
        Collider2D closestCollider = null;
        float closestY = float.MaxValue;

        foreach (Collider2D collider in colliders)
        {
            if (!collider.CompareTag("interactable")) continue;

            float y = collider.transform.position.y;
            if (y < closestY)
            {
                closestY = y;
                closestCollider = collider;
            }
        }

        return closestCollider != null ? closestCollider.transform : null;
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
        if (currentHoverObject == null) return;
        Interactables interactableObject = currentHoverObject.GetComponent<Interactables>();
        if (interactableObject != null) interactableObject.OnDeselectedHover();
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
        lastClickTime = -1f;
        DeselectTarget();
    }
    public void DropHoldObject()
    {
        if (LevelManager.Instance != null && !LevelManager.Instance.IsPlaying) return;
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
    public void FlipHoldTransform(float xMovement, bool isIdle)
    {
        if (isIdle)
        {
            holdTransform.localPosition = new Vector3(0f, 0f, 0);
            return;
        }

        if (xMovement > 0)
        {
            holdTransform.localPosition = new Vector3(0.5f, 0f, 0);
        }
        else
        {
            holdTransform.localPosition = new Vector3(-0.5f, 0f, 0);
        }
        //if (isXPositif) holdTransform.localPosition = new Vector3(0.5f, 0f, 0);
        //else 
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

        if (!(CurrentHeldHoldable is Pet)) return;

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
        lastClickTime = -1f;
    }

    public Transform GetInteractableParent()
    {
        return interactableParent;
    }
}