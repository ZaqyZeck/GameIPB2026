using UnityEngine;

public class FollowerBehavior : IHabitBehavior
{
    private const float FollowDistance = 1f;

    private const float MinFollowDuration = 4f;
    private const float MaxFollowDuration = 8f;
    private const float MinWanderDuration = 3f;
    private const float MaxWanderDuration = 6f;

    private enum State { Following, Wandering }

    private State state;
    private float stateTimer;

    public void OnEnter(Pet pet)
    {
        EnterFollowing();
    }

    public void Tick(Pet pet, float deltaTime)
    {
        stateTimer -= deltaTime;

        switch (state)
        {
            case State.Following:
                TickFollowing(pet);

                if (stateTimer <= 0f)
                {
                    EnterWandering(pet);
                    pet.ChangeTextAction("Wander");
                }
                break;

            case State.Wandering:
                // Head back to following once the wander window is up, or
                // early if it already reached its wander spot and stopped.
                if (stateTimer <= 0f || !pet.Movement.IsMoving)
                {
                    EnterFollowing();
                    pet.ChangeTextAction("Follow player");
                }
                break;
        }
    }

    public void OnExit(Pet pet) => pet.Movement.Stop();

    private void TickFollowing(Pet pet)
    {
        if (PlayerMovement.Instance == null) return;

        Transform player = PlayerMovement.Instance.transform;
        if (Vector3.Distance(pet.transform.position, player.position) > FollowDistance)
        {
            pet.Movement.MoveTo(player.position);
        }
    }

    private void EnterFollowing()
    {
        state = State.Following;
        stateTimer = Random.Range(MinFollowDuration, MaxFollowDuration);
    }

    private void EnterWandering(Pet pet)
    {
        state = State.Wandering;
        stateTimer = Random.Range(MinWanderDuration, MaxWanderDuration);

        Vector3 wanderTarget = PetManager.Instance != null
            ? PetManager.Instance.GetRandomPosition()
            : pet.transform.position;

        pet.Movement.MoveTo(wanderTarget);
    }
}