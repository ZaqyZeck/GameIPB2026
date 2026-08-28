using UnityEngine;

public class CatToyActionBehavior : IActionBehavior
{
    public void Execute(Pet pet)
    {
        pet.Movement.Stop();
        pet.GetPetAnimation().TriggerAction(PetAnimationIds.ActionId_Play);
        Debug.Log($"{pet.name} swats the cat toy!");
    }
}