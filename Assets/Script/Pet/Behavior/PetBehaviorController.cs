using UnityEngine;

public class PetBehaviorController : MonoBehaviour
{
    [SerializeField] private Pet pet;
    private IHabitBehavior currentHabitBehavior;
    private float revealTimer;
    private float habitTimer;
    private bool habitRevealed;
    private bool isDoingAction;
    private bool isDoingHabit;

    [SerializeField] private float habitInterval = 10f;
    public void Initialize(PetData petData)
    {
        habitRevealed = false;
        isDoingAction = false;
        isDoingHabit = false;
        //habitTimer = habitInterval;
        revealTimer = petData.timeToRevealHabit;
        SetHabitBehavior(new IdleHabitBehavior());
    }

    private void Update()
    {
        if (pet.petData == null) return;

        if (!habitRevealed)
        {
            revealTimer -= Time.deltaTime;
            if (revealTimer <= 0f) RevealHabit();
        }
        else if (!isDoingAction)
        {
            habitTimer -= Time.deltaTime;
            if (habitTimer <= 0f)
            {
                currentHabitBehavior?.Tick(pet, Time.deltaTime);
                isDoingHabit = true;
            }
        }

        EnterWandering();
    }

    // Kalau lagi gak Action dan lagi gak Habit aktif (masih Idle), suruh wander.
    private void EnterWandering()
    {
        if (isDoingAction) return;
        
        pet.Movement.WanderAround(3f);
    }

    private void RevealHabit()
    {
        habitRevealed = true;
        SetHabitBehavior(PetBehaviorFactory.GetHabitBehavior(pet.petData.hiddenHabit));
        GameEventBus.OnMemoryUnlocked?.Invoke(pet.petData.hiddenHabit);
    }

    private void SetHabitBehavior(IHabitBehavior newBehavior)
    {
        currentHabitBehavior?.OnExit(pet);
        currentHabitBehavior = newBehavior;
        currentHabitBehavior?.OnEnter(pet);
    }

    public void TryStopAction(ActionTrait trait)
    {
        if (!HasHiddenAction(trait)) return;
        PetBehaviorFactory.GetActionBehavior(trait)?.StopAction(pet);
        isDoingAction = false; // action selesai, buka jalan buat wander/habit lagi
    }

    public void TryExecuteAction(ActionTrait trait)
    {
        if (!HasHiddenAction(trait)) return;
        PetBehaviorFactory.GetActionBehavior(trait)?.ExecuteAction(pet);
        GameEventBus.OnActionExecuted?.Invoke(trait);
        isDoingAction = true; // action sedang jalan, jangan diganggu wander/habit
    }

    public bool HasHiddenAction(ActionTrait trait)
    {
        return habitRevealed && pet.petData != null && pet.petData.hiddenAction == trait;
    }
    public void ResetHabitTimer()
    {
        habitTimer = habitInterval;
        isDoingHabit = false;
    }
}