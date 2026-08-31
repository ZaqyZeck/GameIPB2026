using UnityEngine;

public class SleepyHeadBehavior : IHabitBehavior
{
    private const float SleepInterval = 8f;
    private const float SleepDuration = 5f;

    private float timer;
    private bool isGoingToSleep;
    private bool isSleeping;

    public void OnEnter(Pet pet)
    {
        timer = SleepInterval;
        isGoingToSleep = false;
        isSleeping = false;
    }

    public void Tick(Pet pet, float deltaTime)
    {
        if (isGoingToSleep) return;

        if (isSleeping)
        {
            timer -= deltaTime;

            if (timer <= 0f)
            {
                WakeUp(pet);
            }

            return;
        }

        timer -= deltaTime;

        if (timer <= 0f)
        {
            GoToSleep(pet);
        }
    }

    public void OnExit(Pet pet)
    {
        isGoingToSleep = false;
        isSleeping = false;

        pet.Movement.Stop();
        pet.GetPetAnimation().SetSitting(false);
    }

    private void GoToSleep(Pet pet)
    {
        if (MapManager.Instance == null || MapManager.Instance.bedCollders == null || MapManager.Instance.bedCollders.Length == 0)
        {
            Sleep(pet);
            return;
        }

        isGoingToSleep = true;

        Collider2D bedCollider = MapManager.Instance.bedCollders[Random.Range(0, MapManager.Instance.bedCollders.Length)];
        Vector3 sleepPosition = bedCollider.transform.position;

        pet.ChangeTextAction("goto sleep");
        pet.Movement.MoveTo(sleepPosition, () => Sleep(pet));
    }

    private void Sleep(Pet pet)
    {
        isGoingToSleep = false;
        isSleeping = true;
        timer = SleepDuration;

        pet.ChangeTextAction("sleep");
        pet.Movement.Stop();
        pet.GetPetAnimation().SetSitting(true);

        Debug.LogWarning(pet.petData.petName + " is sleeping");
    }

    private void WakeUp(Pet pet)
    {
        isSleeping = false;
        timer = SleepInterval;

        pet.GetPetAnimation().SetSitting(false);

        Vector3 wakeTarget = PetManager.Instance != null
            ? PetManager.Instance.GetRandomPosition()
            : pet.transform.position;

        pet.ChangeTextAction("wander");
        pet.Movement.MoveTo(wakeTarget);
    }
}