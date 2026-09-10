using System.Collections.Generic;
using UnityEngine;

public class PetIconDatabase : MonoBehaviour
{
    public static PetIconDatabase Instance;

    [SerializeField] private List<PetIconSet> iconSets = new();
    [SerializeField] private List<ActionIconEntry> actionIcons = new();

    private Dictionary<PetType, PetIconSet> typeLookup;
    private Dictionary<ActionTrait, Sprite> actionLookup;

    private void Awake()
    {
        Instance = this;
        BuildLookup();
    }

    private void BuildLookup()
    {
        typeLookup = new Dictionary<PetType, PetIconSet>();
        foreach (PetIconSet set in iconSets)
        {
            if (set == null) continue;
            typeLookup[set.petType] = set;
        }

        actionLookup = new Dictionary<ActionTrait, Sprite>();
        foreach (ActionIconEntry entry in actionIcons)
        {
            actionLookup[entry.action] = entry.icon;
        }
    }

    public Sprite GetTypeIcon(PetType type)
    {
        return typeLookup != null && typeLookup.TryGetValue(type, out PetIconSet set) ? set.typeIcon : null;
    }

    public Sprite GetHabitIcon(PetType type, HabitTrait habit)
    {
        return typeLookup != null && typeLookup.TryGetValue(type, out PetIconSet set) ? set.GetHabitIcon(habit) : null;
    }

    public Sprite GetActionIcon(ActionTrait action)
    {
        return actionLookup != null && actionLookup.TryGetValue(action, out Sprite icon) ? icon : null;
    }
}

[System.Serializable]
public class ActionIconEntry
{
    public ActionTrait action;
    public Sprite icon;
}