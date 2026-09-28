using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TraitDatabase", menuName = "Furgetful/Trait Database")]
public class TraitDatabaseSO : ScriptableObject
{
    public List<TraitSO> traits = new();

    private Dictionary<string, TraitSO> idLookup;
    private Dictionary<ActionTrait, TraitSO> actionLookup;
    private Dictionary<HabitTrait, TraitSO> habitLookup;

    public void InitializeLookup()
    {
        idLookup = new Dictionary<string, TraitSO>();
        actionLookup = new Dictionary<ActionTrait, TraitSO>();
        habitLookup = new Dictionary<HabitTrait, TraitSO>();

        foreach (var trait in traits)
        {
            if (trait == null) continue;

            if (!string.IsNullOrEmpty(trait.traitId))
            {
                idLookup[trait.traitId] = trait;
            }

            if (trait.actionType != ActionTrait.None)
            {
                actionLookup[trait.actionType] = trait;
            }

            if (trait.habitType != HabitTrait.None)
            {
                habitLookup[trait.habitType] = trait;
            }
        }
    }

    public TraitSO GetTraitById(string id)
    {
        if (idLookup == null) InitializeLookup();
        return idLookup != null && idLookup.TryGetValue(id, out var t) ? t : null;
    }

    public TraitSO GetTraitByAction(ActionTrait action)
    {
        if (actionLookup == null) InitializeLookup();
        return actionLookup != null && actionLookup.TryGetValue(action, out var t) ? t : null;
    }

    public TraitSO GetTraitByHabit(HabitTrait habit)
    {
        if (habitLookup == null) InitializeLookup();
        return habitLookup != null && habitLookup.TryGetValue(habit, out var t) ? t : null;
    }

    public List<TraitSO> GetTraitsByCategory(TraitCategory category)
    {
        List<TraitSO> result = new();
        foreach (var trait in traits)
        {
            if (trait != null && trait.category == category)
            {
                result.Add(trait);
            }
        }
        return result;
    }
}
