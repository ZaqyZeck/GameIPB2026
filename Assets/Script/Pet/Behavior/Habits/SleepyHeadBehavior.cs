using UnityEngine;

public class SleepyHeadBehavior : IHabitBehavior
{
    private const float SleepInterval = 8f;
    private const float SleepDuration = 3f;
 
    private float timer;
    private bool isSleeping;
 
    public void OnEnter(Pet pet)
    {
        timer = SleepInterval;
        isSleeping = false;
    }
 
    public void Tick(Pet pet, float deltaTime)
    {
        timer -= deltaTime;
 
        if (!isSleeping && timer <= 0f)
        {
            isSleeping = true;
            timer = SleepDuration;
            pet.Movement.Stop();
            pet.GetPetAnimation().SetSitting(true);
        }
        else if (isSleeping && timer <= 0f)
        {
            isSleeping = false;
            timer = SleepInterval;
            pet.GetPetAnimation().SetSitting(false);
            Vector3 wakeTarget = PetManager.Instance != null
                ? PetManager.Instance.GetRandomPosition()
                : pet.transform.position;
            pet.Movement.MoveTo(wakeTarget);
        }
    }
 
    public void OnExit(Pet pet)
    {
        isSleeping = false;
        pet.GetPetAnimation().SetSitting(false);
    }
}