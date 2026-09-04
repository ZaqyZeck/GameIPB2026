using UnityEngine;

public class FootballActionBehavior : IActionBehavior
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
        //pet.Movement.MoveTo(PetManager.Instance.GetRandomPosition());
        pet.ChangeTextAction("xplay ball");
        pet.Animation.SetSitting(false);
        pet.GetInteractable().StopPlayToy();
    }
    public float GetActionDuration()
    {
        return actionDurtion;
    }
}