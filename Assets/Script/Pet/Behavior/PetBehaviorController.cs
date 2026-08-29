using UnityEngine;

public class PetBehaviorController : MonoBehaviour
{
    [SerializeField] private Pet ghostPet;

    private IHabitBehavior currentHabitBehavior;
    private float revealTimer;
    private bool habitRevealed;

    public void Initialize(PetData petData)
    {
        habitRevealed = false;
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

    // Call this from a toy Interactable when the player uses it near this pet.
    public void TryExecuteAction(ActionTrait trait)
    {
        if (!HasHiddenAction(trait)) return;

        PetBehaviorFactory.GetActionBehavior(trait)?.Execute(ghostPet);
        GameEventBus.OnActionExecuted?.Invoke(trait);
    }

    public bool HasHiddenAction(ActionTrait trait)
    {
        return habitRevealed && ghostPet.petData != null && ghostPet.petData.hiddenAction == trait;
    }
}