using UnityEngine;

public class FishToyActionBehavior : IActionBehavior
{
    public void Execute(Pet pet)
    {
        pet.GetPetAnimation().TriggerAction(PetAnimationIds.ActionId_Play);
        Debug.Log($"{pet.name} chases the fish toy!");
    }
}