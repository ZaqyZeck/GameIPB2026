using UnityEngine;

public class CatToyActionBehavior : IActionBehavior, IDialogueDescribable
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
    }

    public float GetActionDuration()
    {
        return actionDurtion;
    }

    public string GetDialogueText() => "It can't resist swatting at a little toy mouse.";
}