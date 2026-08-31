using UnityEngine;

public class TreasureHunterBehavior : IHabitBehavior
{
    private const float WanderInterval = 10f;
    private const float MinDigDuration = 5f;
    private const float MaxDigDuration = 8f;

    private float timer;

    private bool isGoingToDig;
    private bool isDigging;

    public void OnEnter(Pet pet)
    {
        timer = WanderInterval;
        isGoingToDig = false;
        isDigging = false;
    }

    public void Tick(Pet pet, float deltaTime)
    {
        if (isGoingToDig) return;

        if (isDigging)
        {
            timer -= deltaTime;

            if (timer <= 0f)
            {
                FinishDigging(pet);
            }

            return;
        }

        timer -= deltaTime;

        if (timer <= 0f && !pet.Movement.IsMoving)
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

        // Jalankan animasi digging di sini
        // pet.GetPetAnimation().SetDigging(true);

        Debug.Log($"{pet.petData.petName} started digging");
    }

    private void FinishDigging(Pet pet)
    {
        isDigging = false;
        timer = WanderInterval;

        // Hentikan animasi digging
        // pet.GetPetAnimation().SetDigging(false);

        Debug.Log($"{pet.petData.petName} finished digging");

        Vector3 wanderTarget = PetManager.Instance != null
            ? PetManager.Instance.GetRandomPosition()
            : pet.transform.position;

        pet.ChangeTextAction("WANDER");
        pet.Movement.MoveTo(wanderTarget);
    }
}