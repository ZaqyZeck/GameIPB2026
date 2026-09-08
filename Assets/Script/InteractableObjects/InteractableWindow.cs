using System.Collections.Generic;
using UnityEngine;

public class InteractableWindow : Interactables
{
    [SerializeField] private GameObject windowOpen;
    [SerializeField] private GameObject windowClose;
    [SerializeField] private SpriteRenderer closeWindowRenderer;
    [SerializeField] private Sprite closeWindowHover;
    [SerializeField] private Sprite closeWindowDefault;
    [SerializeField] private float callArea = 5f;
    [SerializeField] private float openDuration = 10f;

    private bool isOpen;
    private float openTimer;

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
        if (MapManager.Instance == null) return;

        List<Pet> petsCalled = PetManager.Instance.GetPetsWithAction(ActionTrait.Sunbathe);

        foreach (Pet pet in petsCalled)
        {
            if (pet == null) continue;

            if (pet.Movement.IsNear(transform.position, callArea))
            {
                pet.ChangeTextAction("goto window");

                pet.Movement.MoveTo(MapManager.Instance.GetRandomPositionIn(MapManager.Instance.sunlightArea), () =>
                {
                    pet.BehaviorController.TryExecuteAction(ActionTrait.Sunbathe);
                });
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
            //Debug.Log("dawdawwdwawda");
            closeWindowRenderer.sprite = closeWindowDefault;
        }
    }
}