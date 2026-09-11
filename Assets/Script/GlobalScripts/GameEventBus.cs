using System;
using UnityEngine;

public static class GameEventBus
{
    public static Action<PetData> OnPetSpawned;
    public static Action<ActionTrait> OnActionExecuted;
    public static Action<HabitTrait> OnMemoryUnlocked;
    public static Action OnReunionSuccess;

    public static Action OnSubmitScore; 
    public static Action OnPause;
    public static Action OnResume;
    public static Action OnWin;

    public static Action<int, int> OnReputationChange;
    public static Action<int, int> OnTakeDamage;
    //Volume
    #region [Audio]
    public static Action<float> onValueChangeMaster;
    public static Action<float> onValueChangeBGM;
    public static Action<float> onValueChangeSFX;
    #endregion
}