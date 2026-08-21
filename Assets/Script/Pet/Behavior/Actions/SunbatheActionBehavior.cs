using UnityEngine;

public class SunbatheActionBehavior : IActionBehavior
{
    public void Execute(GhostPet pet)
    {
        pet.Movement.Stop();
        //gatau juga ini jemur diri tu begimana
        Debug.Log($"{pet.name} sunbathes!");
    }
}
