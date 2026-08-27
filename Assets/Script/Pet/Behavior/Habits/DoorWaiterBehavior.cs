using UnityEngine;

public class DoorWaiterBehavior : IHabitBehavior
{
    public void OnEnter(Pet pet)
    {
        Transform door = PetSpotRegistry.Instance != null ? PetSpotRegistry.Instance.GetDoorSpot() : null;
        if (door != null) pet.Movement.MoveTo(door.position);
    }
 
    public void Tick(Pet pet, float deltaTime) { }
    public void OnExit(Pet pet) { }
}