using UnityEngine;

public class FishToyActionBehavior : IActionBehavior
{
    public void Execute(Pet pet)
    {
        //sekali lagi ini juga taktahu
        Debug.Log($"{pet.name} chases the fish toy!");
    }
}
