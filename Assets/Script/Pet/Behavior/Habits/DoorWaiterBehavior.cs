using UnityEngine;

public class DoorWaiterBehavior : IHabitBehavior
{
    private const float WaitDuration = 5f;
    private const float HabitInterval = 10f;

    private float timer;
    private bool isWaiting;
    private bool isGoingToDoor;
    private bool isDoingHabit;

    public void OnEnter(Pet pet)
    {
        timer = HabitInterval;
        isWaiting = false;
        isGoingToDoor = false;
        isDoingHabit = false;
    }

    public void Tick(Pet pet, float deltaTime)
    {
        if (isGoingToDoor) return;

        timer -= deltaTime;

        if (timer > 0f) return;

        if (isWaiting)
        {
            FinishWaiting(pet);
            EnterWandering(pet);
            return;
        }

        if (!isDoingHabit)
        {
            isDoingHabit = true;
            GoToDoor(pet);
        }
    }

    public void OnExit(Pet pet)
    {
        isWaiting = false;
        isGoingToDoor = false;
        isDoingHabit = false;

        pet.Movement.Stop();
        pet.GetPetAnimation().SetSitting(false);
    }

    private void GoToDoor(Pet pet)
    {
        if (MapManager.Instance == null || MapManager.Instance.doorArea == null)
        {
            EnterWandering(pet);
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

    private void EnterWandering(Pet pet)
    {
        timer = HabitInterval;
        isDoingHabit = false;

        Vector3 wanderTarget = PetManager.Instance != null
            ? PetManager.Instance.GetRandomPosition()
            : pet.transform.position;

        pet.ChangeTextAction("wander");
        pet.Movement.MoveTo(wanderTarget);
    }
}