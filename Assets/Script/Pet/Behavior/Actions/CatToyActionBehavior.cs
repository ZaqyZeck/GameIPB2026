using UnityEngine;

public class CatToyActionBehavior : IActionBehavior
{
    public void Execute(Pet pet)
    {
        //nijuga
        Debug.Log($"{pet.name} swats the cat toy!");
    }
}
