using UnityEngine;

public class DoorWaiterBehavior : IHabitBehavior
{
    public void OnEnter(GhostPet pet)
    {
        Transform door = PetSpotRegistry.Instance != null ? PetSpotRegistry.Instance.GetDoorSpot() : null;
        if (door != null) pet.Movement.MoveTo(door.position);
    }
 
    public void Tick(GhostPet pet, float deltaTime) { }
    public void OnExit(GhostPet pet) { }
}