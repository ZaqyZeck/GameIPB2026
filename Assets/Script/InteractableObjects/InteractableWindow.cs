using System.Collections.Generic;
using UnityEngine;

public class InteractableWindow : Interactables
{
    [SerializeField] private GameObject[] windows;
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
        for (int i = 0; i < windows.Length; i++)
        {
            windows[i].SetActive(true);
        }

        isOpen = true;
        openTimer = openDuration;

        CallPetsAction();
    }

    private void CloseWindow()
    {
        for (int i = 0; i < windows.Length; i++)
        {
            windows[i].SetActive(false);
        }

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
}