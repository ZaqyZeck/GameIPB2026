using UnityEngine;

public class MiceToyActionBehavior : IActionBehavior, IDialogueDescribable
{
    public float actionDurtion = 10f;

    public void ExecuteAction(Pet pet)
    {
        pet.ChangeTextAction("play mice");
        pet.Movement.Stop();
        pet.GetPetAnimation().TriggerAction(PetAnimationIds.ActionId_Play_Tikus);
        Debug.Log($"{pet.name} chases the mice toy!");
    }

    public void StopAction(Pet pet)
    {
        pet.ChangeTextAction("xplay mice");
        pet.GetInteractable().StopPlayToy();
    }

    public float GetActionDuration()
    {
        return actionDurtion;
    }

    public string GetDialogueText() => "A mice toy will get it chasing around instantly.";
}