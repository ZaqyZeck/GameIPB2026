using UnityEngine;

public class FootballActionBehavior : IActionBehavior
{
    public void ExecuteAction(Pet pet)
    {
        pet.Movement.Stop();
        pet.GetPetAnimation().TriggerAction(PetAnimationIds.ActionId_Play);
        Debug.Log($"{pet.name} plays football!");
    }

    public void StopAction(Pet pet)
    {
        //pet.Movement.MoveTo(PetManager.Instance.GetRandomPosition());
        pet.Animation.SetSitting(true);
    }
}