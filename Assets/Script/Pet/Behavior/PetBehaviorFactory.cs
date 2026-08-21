using System;
using System.Collections.Generic;

public static class PetBehaviorFactory
{
    // Habit behaviors hold per-pet state (timers), so we store FACTORY FUNCS
    // and create a fresh instance per pet, never a shared singleton.
    private static readonly Dictionary<HabitTrait, Func<IHabitBehavior>> habitFactories = new()
    {
        { HabitTrait.None,           () => new IdleHabitBehavior() },
        { HabitTrait.SleepyHead,     () => new SleepyHeadBehavior() },
        { HabitTrait.DoorWaiter,     () => new DoorWaiterBehavior() },
        { HabitTrait.TreasureHunter, () => new TreasureHunterBehavior() },
        { HabitTrait.Follower,       () => new FollowerBehavior() },
    };

    // Action behaviors are stateless (Execute only), so singletons are fine.
    private static readonly Dictionary<ActionTrait, IActionBehavior> actionBehaviors = new()
    {
        { ActionTrait.Football, new FootballActionBehavior() },
        { ActionTrait.Sunbathe, new SunbatheActionBehavior() },
        { ActionTrait.FishToy,  new FishToyActionBehavior() },
        { ActionTrait.CatToy,   new CatToyActionBehavior() },
    };

    public static IHabitBehavior GetHabitBehavior(HabitTrait trait)
    {
        return habitFactories.TryGetValue(trait, out var factory)
            ? factory()
            : new IdleHabitBehavior();
    }

    public static IActionBehavior GetActionBehavior(ActionTrait trait)
    {
        return actionBehaviors.TryGetValue(trait, out var behavior) ? behavior : null;
    }
}