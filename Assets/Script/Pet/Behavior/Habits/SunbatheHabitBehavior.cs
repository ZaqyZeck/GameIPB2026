using UnityEngine;

public class SunbatheHabitBehavior : IHabitBehavior, IDialogueDescribable
{
    private const float SunbatheDuration = 8f;

    private float timer;
    private bool isGoingToSunbathe;
    private bool isSunbathing;

    public void OnEnter(Pet pet)
    {
        isGoingToSunbathe = false;
        isSunbathing = false;
    }

    public void Tick(Pet pet, float deltaTime)
    {
        if (isGoingToSunbathe) return;

        if (isSunbathing)
        {
            timer -= deltaTime;

            if (timer <= 0f)
            {
                StopSunbathing(pet);
                StopHabit(pet);
            }

            return;
        }

        if (!IsSunSpotAvailable())
        {
            pet.ChangeTextAction("waiting for sun");
            pet.Movement.WanderAround(3f);
            return;
        }

        timer -= deltaTime;

        if (timer <= 0f)
        {
            GoToSunbathe(pet);
        }
    }

    public void OnExit(Pet pet)
    {
        isGoingToSunbathe = false;
        isSunbathing = false;

        pet.Movement.Stop();
        pet.Animation.SetSitting(false);
    }

    private void StopHabit(Pet pet)
    {
        pet.BehaviorController.ResetHabitTimer();
    }

    private bool IsSunSpotAvailable()
    {
        return InteractableWindow.Instance != null && InteractableWindow.Instance.IsOpen;
    }

    private void GoToSunbathe(Pet pet)
    {
        if (MapManager.Instance == null || MapManager.Instance.sunlightArea == null)
        {
            Sunbathe(pet);
            return;
        }

        isGoingToSunbathe = true;

        Vector3 sunbathePosition = MapManager.Instance.GetRandomPositionIn(MapManager.Instance.sunlightArea);

        pet.ChangeTextAction("goto sunbathe");
        pet.Movement.MoveTo(sunbathePosition, () => Sunbathe(pet));
    }

    private void Sunbathe(Pet pet)
    {
        isGoingToSunbathe = false;
        isSunbathing = true;
        timer = SunbatheDuration;

        pet.ChangeTextAction("sunbathe");
        pet.Movement.Stop();
        pet.Animation.SetSitting(true);

        Debug.Log($"{pet.petData.petName} is sunbathing");
    }

    private void StopSunbathing(Pet pet)
    {
        isSunbathing = false;
        pet.Animation.SetSitting(false);
    }

    public string GetDialogueText() => "It loves finding a warm, sunny spot to lounge in.";
}