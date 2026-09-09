using System;
using UnityEngine;

public static class GameEventBus
{
    public static Action<PetData> OnPetSpawned;
    public static Action<ActionTrait> OnActionExecuted;
    public static Action<HabitTrait> OnMemoryUnlocked;
    public static Action OnReunionSuccess;

    //Volume
    #region [Audio]
    public static Action<float> onValueChangeMaster;
    public static Action<float> onValueChangeBGM;
    public static Action<float> onValueChangeSFX;
    #endregion
}