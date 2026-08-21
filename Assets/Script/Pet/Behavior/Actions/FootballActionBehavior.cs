using UnityEngine;

public class FootballActionBehavior : IActionBehavior
{
    public void Execute(GhostPet pet)
    {
        //masi blom tau ini bakal dilakuinnya tu gmn
        Debug.Log($"{pet.name} plays football!");
    }
}
