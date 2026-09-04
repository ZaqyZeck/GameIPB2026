using UnityEngine;

public class SunbatheActionBehavior : IActionBehavior
{
    public float actionDurtion = 10f;

    public void ExecuteAction(Pet pet)
    {
        pet.ChangeTextAction("sunbath");
        pet.Animation.SetSitting(true);
        //pet.Movement.Stop(5f);

        Debug.Log($"{pet.name} sunbathes!");
    }

    public void StopAction(Pet pet)
    {
        pet.ChangeTextAction("xsunbath");
        pet.Animation.SetSitting(false);
    }
    public float GetActionDuration()
    {
        return actionDurtion;
    }
}
