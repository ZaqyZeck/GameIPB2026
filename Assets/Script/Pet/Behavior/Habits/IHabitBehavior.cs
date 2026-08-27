using UnityEngine;

public interface IHabitBehavior
{
    void OnEnter(Pet pet);
    void Tick(Pet pet, float deltaTime);
    void OnExit(Pet pet);
}