using UnityEngine;

public class CatToyActionBehavior : IActionBehavior
{
    public void ExecuteAction(Pet pet)
    {
        pet.Movement.Stop();
        pet.GetPetAnimation().TriggerAction(PetAnimationIds.ActionId_Play);
        Debug.Log($"{pet.name} swats the cat toy!");
    }

    public void StopAction(Pet pet)
    {
        pet.GetPetAnimation().TriggerAction(PetAnimationIds.ActionId_Play);
    }
}