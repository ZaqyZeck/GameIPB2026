using System;
using UnityEngine;

public static class GameEventBus
{
    public static Action<PetProfileSO> OnPetSpawned;
    public static Action<ActionTrait> OnActionExecuted;
    public static Action<HabitTrait> OnMemoryUnlocked; //p maksud yang ini apa
    public static Action OnReunionSuccess;
}