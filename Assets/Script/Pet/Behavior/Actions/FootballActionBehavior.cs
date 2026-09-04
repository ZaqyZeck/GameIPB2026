using UnityEngine;

public class FootballActionBehavior : IActionBehavior, IDialogueDescribable
{
    public float actionDurtion = 10f;

    public void ExecuteAction(Pet pet)
    {
        pet.ChangeTextAction("play ball");
        pet.Movement.Stop();
        pet.GetPetAnimation().TriggerAction(PetAnimationIds.ActionId_Play);
        Debug.Log($"{pet.name} plays football!");
    }

    public void StopAction(Pet pet)
    {
        pet.ChangeTextAction("xplay ball");
        pet.Animation.SetSitting(false);
        pet.GetInteractable().StopPlayToy();
    }

    public float GetActionDuration()
    {
        return actionDurtion;
    }

    public string GetDialogueText() => "Give it a ball and it'll play for hours.";
}