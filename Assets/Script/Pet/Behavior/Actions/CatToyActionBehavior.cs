using UnityEngine;

public class CatToyActionBehavior : IActionBehavior
{
    public void Execute(GhostPet pet)
    {
        //nijuga
        Debug.Log($"{pet.name} swats the cat toy!");
    }
}
