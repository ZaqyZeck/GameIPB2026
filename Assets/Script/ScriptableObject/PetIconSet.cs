using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPetIconSet", menuName = "Pet Icon Set")]
public class PetIconSet : ScriptableObject
{
    public PetType petType;
    public Sprite typeIcon;

    public List<HabitIconEntry> habitIcons = new();

    public Sprite GetHabitIcon(HabitTrait habit)
    {
        foreach (HabitIconEntry entry in habitIcons)
        {
            if (entry.habit == habit) return entry.icon;
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