using UnityEngine;

public class FishToyActionBehavior : IActionBehavior
{
    public void ExecuteAction(Pet pet)
    {
        pet.ChangeTextAction("play fish");
        pet.GetPetAnimation().TriggerAction(PetAnimationIds.ActionId_Play);
        Debug.Log($"{pet.name} chases the fish toy!");
    }

    public void StopAction(Pet pet)
    {
        pet.ChangeTextAction("xplay fish");
        throw new System.NotImplementedException();
    }
}