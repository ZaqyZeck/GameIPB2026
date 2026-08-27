using UnityEngine;

public class TreasureHunterBehavior : IHabitBehavior
{
    private const float WanderInterval = 6f;
    private float timer;
 
    public void OnEnter(Pet pet) => timer = 0f;
 
    public void Tick(Pet pet, float deltaTime)
    {
        timer -= deltaTime;
 
        if (timer <= 0f && !pet.Movement.IsMoving && PetSpotRegistry.Instance != null)
        {
            timer = WanderInterval;
            pet.Movement.MoveTo(PetSpotRegistry.Instance.GetRandomDigSpot());
            // animationanimationanimationnnn
        }
    }
 
    public void OnExit(Pet pet) { }
}