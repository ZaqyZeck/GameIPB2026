using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPetIconSet", menuName = "Pet Icon Set")]
public class PetIconSet : ScriptableObject
{
    public PetType petType;
    public Sprite typeIcon;

    public List<HabitIconEntry> habitIcons = new();
    public List<ActionIconEntry> actionIcons = new();

    public Sprite GetHabitIcon(HabitTrait habit)
    {
        foreach (HabitIconEntry entry in habitIcons)
        {
            if (entry.habit == habit) return entry.icon;
        }
        return null;
    }

    public Sprite GetActionIcon(ActionTrait action)
    {
        foreach (ActionIconEntry entry in actionIcons)
        {
            if (entry.action == action) return entry.icon;
        }
        return null;
    }
}

[System.Serializable]
public class HabitIconEntry
{
    public HabitTrait habit;
    public Sprite icon;
}

[System.Serializable]
public class ActionIconEntry
{
    public ActionTrait action;
    public Sprite icon;
}