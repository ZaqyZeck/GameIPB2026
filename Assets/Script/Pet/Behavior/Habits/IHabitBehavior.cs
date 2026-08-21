using UnityEngine;

public interface IHabitBehavior
{
    void OnEnter(GhostPet pet);
    void Tick(GhostPet pet, float deltaTime);
    void OnExit(GhostPet pet);
}