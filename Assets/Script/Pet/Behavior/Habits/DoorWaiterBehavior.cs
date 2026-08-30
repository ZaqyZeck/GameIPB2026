using UnityEngine;

public class DoorWaiterBehavior : IHabitBehavior
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

        GoToDoor(pet);
    }

    public void Tick(Pet pet, float deltaTime)
    {
        if (isGoingToDoor) return;

        if (!isWaiting) return;

        timer -= deltaTime;

        if (timer <= 0f)
        {
            FinishWaiting(pet);
        }
    }

    public void OnExit(Pet pet)
    {
        isWaiting = false;
        isGoingToDoor = false;

        pet.Movement.Stop();
        pet.GetPetAnimation().SetSitting(false);
    }

    private void GoToDoor(Pet pet)
    {
        if (MapManager.Instance == null || MapManager.Instance.doorArea == null)
        {
            FinishWaiting(pet);
            return;
        }

        isGoingToDoor = true;

        Vector3 targetPosition = MapManager.Instance.GetRandomPositionIn(MapManager.Instance.doorArea);

        pet.Movement.MoveTo(targetPosition, () => StartWaiting(pet));
    }

    private void StartWaiting(Pet pet)
    {
        isGoingToDoor = false;
        isWaiting = true;
        timer = WaitDuration;

        pet.Movement.Stop();
        pet.GetPetAnimation().SetSitting(true);
    }

    private void FinishWaiting(Pet pet)
    {
        isWaiting = false;

        pet.GetPetAnimation().SetSitting(false);
    }
}