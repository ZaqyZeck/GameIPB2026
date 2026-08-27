using UnityEngine;

public class FollowerBehavior : IHabitBehavior
{
    private const float FollowDistance = 1f;
 
    public void OnEnter(Pet pet) { }
 
    public void Tick(Pet pet, float deltaTime)
    {
        if (PlayerMovement.Instance == null) return;
 
        Transform player = PlayerMovement.Instance.transform;
        if (Vector3.Distance(pet.transform.position, player.position) > FollowDistance)
        {
            pet.Movement.MoveTo(player.position);
        }
    }
 
    public void OnExit(Pet pet) => pet.Movement.Stop();
}