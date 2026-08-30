using System.Collections.Generic;
using UnityEngine;

public class InteractableWindow : Interactables
{
    bool isOpen;
    [SerializeField] private GameObject[] windows;
    //List<Pet> petsCalled;
    [SerializeField] float callArea;
    public override void OnInteract(PlayerInteract player)
    {
        if (isOpen) CloseWindow();
        else OpenWindow();
    }

    void CloseWindow()
    {
        for (int i = 0; i < windows.Length; i++)
        {
            windows[i].SetActive(false);
        }
        isOpen = false;
    }

    void OpenWindow()
    {
        for (int i = 0; i < windows.Length; i++)
        {
            windows[i].SetActive(true);
        }

        isOpen = true;
        CallPetsAction();
    }

    void CallPetsAction()
    {
        List<Pet>  petsCalled = PetManager.Instance.GetPetsWithAction(ActionTrait.Sunbathe);
        foreach (Pet pet in petsCalled)
        {
            if (pet == null) continue;
            if (pet.Movement.IsNear(transform.position, callArea))
                pet.Movement.MoveTo(MapManager.Instance.GetRandomPositionIn(MapManager.Instance.sunlightArea), () =>
                {
                    pet.BehaviorController.TryExecuteAction(ActionTrait.Sunbathe);
                });
        }
    }
}
