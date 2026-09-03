using UnityEngine;

public class FollowerBehavior : IHabitBehavior
{
    private const float FollowDistance = 1f;

    private const float MinFollowDuration = 4f;
    private const float MaxFollowDuration = 8f;
    //private const float MinWanderDuration = 3f;
    //private const float MaxWanderDuration = 6f;

    //private enum State { Following, Wandering }

    //private State state;
    private float followTimer;

    public void OnEnter(Pet pet)
    {
        ResetTimer();
    }

    public void Tick(Pet pet, float deltaTime)
    {
        followTimer -= deltaTime;

        TickFollowing(pet);

        if (followTimer <= 0f)
        {
            StopHabit(pet);
            pet.ChangeTextAction("Wander");
        }

        //EnterFollowing();
        pet.ChangeTextAction("Follow player");

    }

    public void OnExit(Pet pet) => pet.Movement.Stop();
    private void StopHabit(Pet pet)
    {
        ResetTimer();
        pet.BehaviorController.ResetHabitTimer();
    }

    private void TickFollowing(Pet pet)
    {
        if (PlayerMovement.Instance == null) return;

        Transform player = PlayerMovement.Instance.transform;
        if (Vector3.Distance(pet.transform.position, player.position) > FollowDistance)
        {
            pet.Movement.MoveTo(player.position);
        }
    }

    private void ResetTimer()
    {
        //state = State.Following;
        followTimer = Random.Range(MinFollowDuration, MaxFollowDuration);
    }

    //private void EnterWandering(Pet pet)
    //{
    //    state = State.Wandering;
    //    stateTimer = Random.Range(MinWanderDuration, MaxWanderDuration);

    //    Vector3 wanderTarget = PetManager.Instance != null
    //        ? PetManager.Instance.GetRandomPosition()
    //        : pet.transform.position;

    //    pet.Movement.MoveTo(wanderTarget);
    //}
}