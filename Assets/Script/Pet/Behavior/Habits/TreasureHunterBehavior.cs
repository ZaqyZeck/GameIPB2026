using UnityEngine;

public class TreasureHunterBehavior : IHabitBehavior, IDialogueDescribable
{
    private const float MinDigDuration = 5f;
    private const float MaxDigDuration = 8f;

    private float timer;

    private bool isGoingToDig;
    private bool isDigging;

    public void OnEnter(Pet pet)
    {
        isGoingToDig = false;
        isDigging = false;
    }

    public void Tick(Pet pet, float deltaTime)
    {
        if (isGoingToDig) return;
        timer -= deltaTime;

        if (isDigging)
        {
            if (timer <= 0f)
            {
                FinishDigging(pet);
                StopHabit(pet);
            }
            return;
        }

        if (timer <= 0f)
        {
            GoToDigSpot(pet);
        }
    }

    public void OnExit(Pet pet)
    {
        isGoingToDig = false;
        isDigging = false;

        pet.Movement.Stop();
    }

    private void StopHabit(Pet pet)
    {
        pet.BehaviorController.ResetHabitTimer();
    }

    private void GoToDigSpot(Pet pet)
    {
        if (PetSpotRegistry.Instance == null) return;

        isGoingToDig = true;

        Vector3 digPosition = PetSpotRegistry.Instance.GetRandomDigSpot();

        pet.ChangeTextAction("goto dig");
        pet.Movement.MoveTo(digPosition, () => StartDigging(pet));
    }

    private void StartDigging(Pet pet)
    {
        isGoingToDig = false;
        isDigging = true;
        timer = Random.Range(MinDigDuration, MaxDigDuration);

        pet.ChangeTextAction("DIGGING");
        pet.Movement.Stop();

        Debug.Log($"{pet.petData.petName} started digging");
    }

    private void FinishDigging(Pet pet)
    {
        isDigging = false;
        Debug.Log($"{pet.petData.petName} finished digging");
    }

    public string GetDialogueText() => "It's always digging around, looking for buried treasure.";
}