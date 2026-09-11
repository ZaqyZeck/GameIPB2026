using System.Collections.Generic;
using UnityEngine;

public class InteractableWindow : Interactables
{
    public static InteractableWindow Instance;

    [SerializeField] private GameObject windowOpen;
    [SerializeField] private GameObject windowClose;
    [SerializeField] private SpriteRenderer closeWindowRenderer;
    [SerializeField] private Sprite closeWindowHover;
    [SerializeField] private Sprite closeWindowDefault;
    [SerializeField] private float callArea = 5f;
    [SerializeField] private float openDuration = 10f;

    private bool isOpen;
    private float openTimer;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!isOpen) return;

        openTimer -= Time.deltaTime;

        if (openTimer <= 0f)
        {
            CloseWindow();
        }
    }

    public override void OnInteract(PlayerInteract player)
    {
        if (!isOpen)
        {
            OpenWindow();
        }
    }

    private void OpenWindow()
    {
        windowOpen.SetActive(true);
        windowClose.SetActive(false);

        isOpen = true;
        openTimer = openDuration;

        CallPetsAction();
    }

    private void CloseWindow()
    {
        windowOpen.SetActive(false);
        windowClose.SetActive(true);

        isOpen = false;
        openTimer = 0f;
    }

    private void CallPetsAction()
    {
        if (PetManager.Instance == null) return;

        List<Pet> petsCalled = PetManager.Instance.GetPetsWithHabit(HabitTrait.Sunbathe);

        foreach (Pet pet in petsCalled)
        {
            if (pet == null) continue;

            if (pet.Movement.IsNear(transform.position, callArea))
            {
                pet.BehaviorController.TryExecuteHabit(HabitTrait.Sunbathe);
            }
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

    void TurnOnOutline(bool isOn)
    {
        if (closeWindowRenderer == null) return;
        if (isOn)
        {
            closeWindowRenderer.sprite = closeWindowHover;
        }
        else
        {
            closeWindowRenderer.sprite = closeWindowDefault;
        }
    }
}