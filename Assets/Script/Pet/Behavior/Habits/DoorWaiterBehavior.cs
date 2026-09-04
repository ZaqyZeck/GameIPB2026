using UnityEngine;

public class DoorWaiterBehavior : IHabitBehavior, IDialogueDescribable
{
    private const float WaitDuration = 5f;

    private float timer;
    private bool isWaiting;
    private bool isGoingToDoor;

    public void OnEnter(Pet pet)
    {
        timer = 0f;
        isWaiting = false;
        isGoingToDoor = false;
    }

    public void Tick(Pet pet, float deltaTime)
    {
        if (isGoingToDoor) return;

        timer -= deltaTime;

        if (timer > 0f) return;

        if (isWaiting)
        {
            FinishWaiting(pet);
            StopHabit(pet);
            return;
        }

        GoToDoor(pet);
    }

    public void OnExit(Pet pet)
    {
        isWaiting = false;
        isGoingToDoor = false;

        pet.Movement.Stop();
        pet.GetPetAnimation().SetSitting(false);
    }

    private void StopHabit(Pet pet)
    {
        pet.BehaviorController.ResetHabitTimer();
    }

    private void GoToDoor(Pet pet)
    {
        if (MapManager.Instance == null || MapManager.Instance.doorArea == null)
        {
            StopHabit(pet);
            return;
        }

        isGoingToDoor = true;

        Vector3 targetPosition = MapManager.Instance.GetRandomPositionIn(MapManager.Instance.doorArea);

        pet.ChangeTextAction("go door");
        pet.Movement.MoveTo(targetPosition, () => StartWaiting(pet));
    }

    private void StartWaiting(Pet pet)
    {
        isGoingToDoor = false;
        isWaiting = true;
        timer = WaitDuration;

        pet.ChangeTextAction("wait door");
        pet.Movement.Stop();
        pet.GetPetAnimation().SetSitting(true);
    }

    private void FinishWaiting(Pet pet)
    {
        isWaiting = false;
        pet.GetPetAnimation().SetSitting(false);
    }

    public string GetDialogueText() => "It likes to sit by the door, waiting for someone to come home.";
}