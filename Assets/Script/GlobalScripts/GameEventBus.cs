using System;
using UnityEngine;

public static class GameEventBus
{
    public static Action<PetData> OnPetSpawned;
    public static Action<ActionTrait> OnActionExecuted;
    public static Action<HabitTrait> OnMemoryUnlocked; 
    public static Action OnReunionSuccess;
}