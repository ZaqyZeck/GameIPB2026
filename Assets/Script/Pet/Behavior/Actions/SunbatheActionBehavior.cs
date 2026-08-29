using UnityEngine;

public class SunbatheActionBehavior : IActionBehavior
{
    public void ExecuteAction(Pet pet)
    {
        pet.Movement.Stop();
        //gatau juga ini jemur diri tu begimana
        Debug.Log($"{pet.name} sunbathes!");
    }

    public void StopAction(Pet pet)
    {
        throw new System.NotImplementedException();
    }
}
