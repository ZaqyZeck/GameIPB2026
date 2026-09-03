using UnityEngine;

public class PetBehaviorController : MonoBehaviour
{
    [SerializeField] private Pet pet;
    [SerializeField] private float habitInterval = 10f;

    private IHabitBehavior currentHabitBehavior;

    private float revealTimer;
    private float habitTimer;
    private float actionTimer;

    private bool habitRevealed;
    private bool isDoingAction;
    private bool isDoingHabit;
    private bool isStopBehaviour;

    public void Initialize(PetData petData)
    {
        habitRevealed = false;
        isDoingAction = false;
        isDoingHabit = false;
        isStopBehaviour = false;

        revealTimer = petData.timeToRevealHabit;
        habitTimer = habitInterval;

        SetHabitBehavior(new IdleHabitBehavior());
    }

    private void Update()
    {
        if (pet.petData == null || isStopBehaviour) return;
        if (pet.GetInteractable().isPickuped) return;

        if (!habitRevealed)
        {
            revealTimer -= Time.deltaTime;

            if (revealTimer <= 0f)
            {
                RevealHabit();
            }

            EnterWandering();
            return;
        }

        if (isDoingAction)
        {
            UpdateAction();
            return;
        }

        if (isDoingHabit)
        {
            currentHabitBehavior?.Tick(pet, Time.deltaTime);
            return;
        }

        UpdateHabitTimer();
    }

    private void UpdateAction()
    {
        actionTimer -= Time.deltaTime;

        if (actionTimer <= 0f)
        {
            TryStopAction(pet.petData.hiddenAction);
        }
    }

    private void UpdateHabitTimer()
    {
        habitTimer -= Time.deltaTime;

        if (habitTimer <= 0f)
        {
            StartHabit();
            return;
        }

        EnterWandering();
    }

    private void StartHabit()
    {
        isDoingHabit = true;
        currentHabitBehavior?.Tick(pet, Time.deltaTime);
    }

    private void EnterWandering()
    {
        if (isDoingAction || isDoingHabit || isStopBehaviour) return;

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

    public void TryExecuteAction(ActionTrait trait)
    {
        if (isStopBehaviour) return;
        if (!HasHiddenAction(trait)) return;

        IActionBehavior actionBehaviour = PetBehaviorFactory.GetActionBehavior(trait);

        if (actionBehaviour == null) return;

        if (isDoingHabit)
        {
            currentHabitBehavior?.OnExit(pet);
            isDoingHabit = false;
        }

        pet.Movement.Stop();

        actionBehaviour.ExecuteAction(pet);

        actionTimer = actionBehaviour.GetActionDuration();
        isDoingAction = true;

        GameEventBus.OnActionExecuted?.Invoke(trait);
    }

    public void TryStopAction(ActionTrait trait)
    {
        if (!isDoingAction) return;
        if (!HasHiddenAction(trait)) return;

        IActionBehavior actionBehaviour = PetBehaviorFactory.GetActionBehavior(trait);

        actionBehaviour?.StopAction(pet);

        isDoingAction = false;
        actionTimer = 0f;

        habitTimer = habitInterval;
        isDoingHabit = false;
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

    public void PickUpBehaviour(ActionTrait trait)
    {
        if (isStopBehaviour) return;

        isStopBehaviour = true;

        StopAllBehaviour(trait);
    }

    public void DropBehaviour()
    {
        isStopBehaviour = false;

        habitTimer = habitInterval;
        isDoingHabit = false;
        isDoingAction = false;
        actionTimer = 0f;

        pet.Movement.Stop();
    }

    public void StopAllBehaviour(ActionTrait trait)
    {
        if (isDoingAction)
        {
            TryStopAction(trait);
        }

        if (isDoingHabit)
        {
            currentHabitBehavior?.OnExit(pet);
            isDoingHabit = false;
        }

        pet.Movement.Stop();
    }
}