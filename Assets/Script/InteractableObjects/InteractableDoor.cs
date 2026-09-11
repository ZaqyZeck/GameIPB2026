using UnityEngine;

public class InteractableDoor : Interactables
{
    [SerializeField] private GameObject doorOpen;
    [SerializeField] private GameObject doorClose;

    [SerializeField] private float openDuration = 2f;
    [SerializeField] private Material doorMaterial;
    [SerializeField] private GameObject doorBell;

    private bool isOpen;
    private float openTimer;

    private void Update()
    {
        if (PetManager.Instance.isPetsAtDoor)
        {
            if (doorBell != null) doorBell.SetActive(true);
        }
        else
        {
            if (doorBell != null) doorBell.SetActive(false);
        }

        if (!isOpen) return;

        openTimer -= Time.deltaTime;

        if (openTimer <= 0f)
        {
            CloseDoor();
        }
    }

    private void Start()
    {
        if (doorMaterial != null) doorMaterial.SetFloat("_outlineOn", 0f);
    }

    public override void OnInteract(PlayerInteract player)
    {
        OpenDoor();
    }

    private void OpenDoor()
    {
        if (!PetManager.Instance.isPetsAtDoor || isOpen) return;
        doorOpen.SetActive(true);
        doorClose.SetActive(false);
        if (doorBell != null) doorBell.SetActive(false);

        isOpen = true;
        openTimer = openDuration;

        PetManager.Instance.DoorOpen();
        TurnOnOutline(false);
    }

    private void CloseDoor()
    {
        doorOpen.SetActive(false);
        doorClose.SetActive(true);

        isOpen = false;
        openTimer = 0f;
    }

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
        if (doorMaterial == null) return;
        if (isOn)
        {
            doorMaterial.SetFloat("_outlineOn", 1.0f);
        }
        else
        {
            //Debug.Log("dawdawwdwawda");
            doorMaterial.SetFloat("_outlineOn", 0f);
        }
    }

    //public void TurnOnBell
}
