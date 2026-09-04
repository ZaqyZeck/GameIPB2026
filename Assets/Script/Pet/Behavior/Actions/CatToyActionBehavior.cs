using UnityEngine;

public class CatToyActionBehavior : IActionBehavior
{
    public float actionDurtion = 10f;

    public void ExecuteAction(Pet pet)
    {
        pet.ChangeTextAction("play mouse");
        pet.Movement.Stop();
        pet.GetPetAnimation().TriggerAction(PetAnimationIds.ActionId_Play);
        Debug.Log($"{pet.name} swats the cat toy!");
    }

    public void StopAction(Pet pet)
    {
        pet.ChangeTextAction("xplay mouse");
        pet.Animation.SetSitting(false);
        pet.GetInteractable().StopPlayToy();
        //pet.GetPetAnimation().TriggerAction(PetAnimationIds.ActionId_Play);
        //pet.GetPetAnimation().Se
    }

    public float GetActionDuration()
    {
        return actionDurtion;
    }
}