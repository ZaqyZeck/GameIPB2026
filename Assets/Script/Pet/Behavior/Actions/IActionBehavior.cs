using UnityEngine;

public interface IActionBehavior
{
    void StopAction(Pet pet);
    void ExecuteAction(Pet pet);
    float GetActionDuration();
}