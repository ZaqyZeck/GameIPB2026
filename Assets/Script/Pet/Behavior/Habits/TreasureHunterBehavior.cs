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
        Debug.Log("treasure stop scrtaing");
        pet.BehaviorController.ResetHabitTimer();
    }

    private void GoToDigSpot(Pet pet)
    {
        if (MapManager.Instance == null || MapManager.Instance.treasureHuntArea == null || MapManager.Instance.treasureHuntArea.Length == 0)
        {
            Debug.LogError("gak ada treasure hunt area buat habit");
            return;
        }

        isGoingToDig = true;

        Vector3 digPosition = MapManager.Instance.GetRandomPositionInArray(MapManager.Instance.treasureHuntArea);

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
        pet.Animation.TriggerAction(PetAnimationIds.ActionId_Claw);
        Debug.Log($"{pet.petData.petName} started digging");
    }

    private void FinishDigging(Pet pet)
    {
        isDigging = false;
        pet.Animation.ResetAction();
        Debug.Log($"{pet.petData.petName} finished digging");
    }

    public string GetDialogueText() => "Every so often it claws at a wall, convinced there's treasure buried inside.";
}