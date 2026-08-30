using UnityEngine;

public class SunbatheActionBehavior : IActionBehavior
{
    public void ExecuteAction(Pet pet)
    {
        pet.Animation.SetSitting(true);
        pet.Movement.Stop(5f);

        Debug.Log($"{pet.name} sunbathes!");
    }

    public void StopAction(Pet pet)
    {
        throw new System.NotImplementedException();
    }
}
