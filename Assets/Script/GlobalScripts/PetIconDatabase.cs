using System.Collections.Generic;
using UnityEngine;

public class PetIconDatabase : MonoBehaviour
{
    public static PetIconDatabase Instance;

    [SerializeField] private List<PetIconSet> iconSets = new();

    private Dictionary<PetType, PetIconSet> lookup;

    private void Awake()
    {
        Instance = this;
        BuildLookup();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<PetType, PetIconSet>();

        foreach (PetIconSet set in iconSets)
        {
            if (set == null) continue;
            lookup[set.petType] = set;
        }
    }

    public Sprite GetTypeIcon(PetType type)
    {
        return lookup != null && lookup.TryGetValue(type, out PetIconSet set) ? set.typeIcon : null;
    }

    public Sprite GetHabitIcon(PetType type, HabitTrait habit)
    {
        return lookup != null && lookup.TryGetValue(type, out PetIconSet set) ? set.GetHabitIcon(habit) : null;
    }

    public Sprite GetActionIcon(PetType type, ActionTrait action)
    {
        return lookup != null && lookup.TryGetValue(type, out PetIconSet set) ? set.GetActionIcon(action) : null;
    }
}