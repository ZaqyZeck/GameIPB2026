using UnityEngine;

public class FollowerBehavior : IHabitBehavior
{
    private const float FollowDistance = 1f;
 
    public void OnEnter(GhostPet pet) { }
 
    public void Tick(GhostPet pet, float deltaTime)
    {
        if (PlayerMovement.Instance == null) return;
 
        Transform player = PlayerMovement.Instance.transform;
        if (Vector3.Distance(pet.transform.position, player.position) > FollowDistance)
        {
            pet.Movement.MoveTo(player.position);
        }
    }
 
    public void OnExit(GhostPet pet) => pet.Movement.Stop();
}