using UnityEngine;

public class SunbatheActionBehavior : IActionBehavior
{
    public void Execute(Pet pet)
    {
        pet.Movement.Stop();
        //gatau juga ini jemur diri tu begimana
        Debug.Log($"{pet.name} sunbathes!");
    }
}
