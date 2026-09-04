public class IdleHabitBehavior : IHabitBehavior, IDialogueDescribable
{
    public void OnEnter(Pet pet) { }
    public void Tick(Pet pet, float deltaTime) { }
    public void OnExit(Pet pet) { }

    public string GetDialogueText() => "It doesn't seem to have any particular habits... yet.";
}