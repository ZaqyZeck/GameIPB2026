using UnityEngine;

public class PetBehaviorController : MonoBehaviour
{
    [SerializeField] private Pet ghostPet;
    private IHabitBehavior currentHabitBehavior;
    private float revealTimer;
    private bool habitRevealed;
    private bool isPerformingAction;

    public void Initialize(PetData petData)
    {
        habitRevealed = false;
        isPerformingAction = false;
        revealTimer = petData.timeToRevealHabit;
        SetHabitBehavior(new IdleHabitBehavior());
    }

    private void Update()
    {
        if (ghostPet.petData == null) return;

        if (!habitRevealed)
        {
            revealTimer -= Time.deltaTime;
            if (revealTimer <= 0f) RevealHabit();
        }

        currentHabitBehavior?.Tick(ghostPet, Time.deltaTime);

        UpdateIdleWander();
    }

    // Kalau lagi gak Action dan lagi gak Habit aktif (masih Idle), suruh wander.
    private void UpdateIdleWander()
    {
        if (isPerformingAction) return;
        if (currentHabitBehavior is IdleHabitBehavior)
        {
            ghostPet.Movement.WanderAround();
        }
    }

    private void RevealHabit()
    {
        habitRevealed = true;
        SetHabitBehavior(PetBehaviorFactory.GetHabitBehavior(ghostPet.petData.hiddenHabit));
        GameEventBus.OnMemoryUnlocked?.Invoke(ghostPet.petData.hiddenHabit);
    }

    private void SetHabitBehavior(IHabitBehavior newBehavior)
    {
        currentHabitBehavior?.OnExit(ghostPet);
        currentHabitBehavior = newBehavior;
        currentHabitBehavior?.OnEnter(ghostPet);
    }

    public void TryStopAction(ActionTrait trait)
    {
        if (!HasHiddenAction(trait)) return;
        PetBehaviorFactory.GetActionBehavior(trait)?.StopAction(ghostPet);
        isPerformingAction = false; // action selesai, buka jalan buat wander/habit lagi
    }

    public void TryExecuteAction(ActionTrait trait)
    {
        if (!HasHiddenAction(trait)) return;
        PetBehaviorFactory.GetActionBehavior(trait)?.ExecuteAction(ghostPet);
        GameEventBus.OnActionExecuted?.Invoke(trait);
        isPerformingAction = true; // action sedang jalan, jangan diganggu wander/habit
    }

    public bool HasHiddenAction(ActionTrait trait)
    {
        return habitRevealed && ghostPet.petData != null && ghostPet.petData.hiddenAction == trait;
    }
}