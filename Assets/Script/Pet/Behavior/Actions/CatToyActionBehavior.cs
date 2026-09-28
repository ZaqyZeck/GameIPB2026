using UnityEngine;

public class CatToyActionBehavior : IActionBehavior
{
    public float actionDurtion = 10f;

    public void ExecuteAction(Pet pet)
    {
        pet.ChangeTextAction("play mouse");
        pet.Movement.Stop();
        pet.GetPetAnimation().TriggerAction(PetAnimationIds.ActionId_Play_Bulu);
        pet.ShowActionIcon(ActionTrait.CatToy);
        pet.ShowCatToyProp();
        Debug.Log($"{pet.name} swats the cat toy!");
    }

    public void StopAction(Pet pet)
    {
        pet.ChangeTextAction("xplay mouse");
        pet.Animation.SetSitting(false);
        pet.Animation.ResetAction();
        pet.HideActionIcon();
        pet.HideCatToyProp();
        pet.GetInteractable().StopPlayToy();
    }

    public float GetActionDuration()
    {
        return actionDurtion;
    }
}