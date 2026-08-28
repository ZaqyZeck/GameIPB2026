using UnityEngine;

public class FootballActionBehavior : IActionBehavior
{
    public void Execute(Pet pet)
    {
        pet.Movement.Stop();
        pet.GetPetAnimation().TriggerAction(PetAnimationIds.ActionId_Play);
        Debug.Log($"{pet.name} plays football!");
    }
}