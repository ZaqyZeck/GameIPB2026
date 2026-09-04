using UnityEngine;

public class FollowerBehavior : IHabitBehavior, IDialogueDescribable
{
    private const float FollowDistance = 1f;

    private const float MinFollowDuration = 4f;
    private const float MaxFollowDuration = 8f;

    private float followTimer;

    public void OnEnter(Pet pet)
    {
        ResetTimer();
    }

    public void Tick(Pet pet, float deltaTime)
    {
        followTimer -= deltaTime;

        if (followTimer <= 0f)
        {
            StopHabit(pet);
            pet.ChangeTextAction("Wander");
            return;
        }

        TickFollowing(pet);
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
        followTimer = Random.Range(MinFollowDuration, MaxFollowDuration);
    }

    public string GetDialogueText() => "It tends to follow people around wherever they go.";
}