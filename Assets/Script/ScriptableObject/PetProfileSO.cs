using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPetProfile", menuName = "Pet Profile")]
public class PetProfileSO : ScriptableObject
{
    public List<PetData> petDatas = new();

    [Header("Trait Roll Pools")]
    [Tooltip("Pool of colors that can be randomly assigned to a spawned pet. If empty, the pet's authored specialColor is used instead.")]
    public Color[] colorPool;

    [Tooltip("Pool of owner sprites that can be randomly assigned to a spawned pet's future owner.")]
    public Sprite[] ownerSpritePool;
}

[Serializable]
public class PetData
{
    [Header("Identitas Arwah")]
    public string petName = "Unknown Soul";
    public Sprite petSprite; // Base appearance
    public PetSpecies species;
    public PetType petType;

    [Header("Ciri-Ciri Kasat Mata (Immediate)")]
    public VisualTrait visualTrait;
    public Color specialColor = Color.white;
    public Sprite ownerSprite;

    [Header("Ciri-Ciri Habits (Passive/Timer)")]
    public HabitTrait hiddenHabit;
    public float timeToRevealHabit = 15f;

    [Header("Ciri-Ciri Action (Interactive)")]
    public ActionTrait hiddenAction;

    [Header("Preferences (Data-Driven)")]
    public List<PetPreference> preferences = new();

    public PetData Clone()
    {
        var clone = new PetData
        {
            petName = petName,
            petSprite = petSprite,
            species = species,
            petType = petType,
            visualTrait = visualTrait,
            specialColor = specialColor,
            ownerSprite = ownerSprite,
            hiddenHabit = hiddenHabit,
            timeToRevealHabit = timeToRevealHabit,
            hiddenAction = hiddenAction
        };

        if (preferences != null)
        {
            foreach (var pref in preferences)
            {
                if (pref != null)
                {
                    clone.preferences.Add(new PetPreference(pref.trait, pref.preference));
                }
            }
        }

        return clone;
    }

    public bool LikesAction(ActionTrait action)
    {
        if (hiddenAction == action) return true;
        if (preferences != null)
        {
            foreach (var pref in preferences)
            {
                if (pref != null && pref.trait != null && pref.trait.actionType == action && pref.preference == PreferenceType.Like)
                    return true;
            }
        }
        return false;
    }

    public bool DislikesAction(ActionTrait action)
    {
        if (preferences != null)
        {
            foreach (var pref in preferences)
            {
                if (pref != null && pref.trait != null && pref.trait.actionType == action && pref.preference == PreferenceType.Dislike)
                    return true;
            }
        }
        return false;
    }

    public bool LikesHabit(HabitTrait habit)
    {
        if (hiddenHabit == habit) return true;
        if (preferences != null)
        {
            foreach (var pref in preferences)
            {
                if (pref != null && pref.trait != null && pref.trait.habitType == habit && pref.preference == PreferenceType.Like)
                    return true;
            }
        }
        return false;
    }

    public bool DislikesHabit(HabitTrait habit)
    {
        if (preferences != null)
        {
            foreach (var pref in preferences)
            {
                if (pref != null && pref.trait != null && pref.trait.habitType == habit && pref.preference == PreferenceType.Dislike)
                    return true;
            }
        }
        return false;
    }

    public void SetPreference(TraitSO trait, PreferenceType type)
    {
        if (trait == null) return;
        if (preferences == null) preferences = new List<PetPreference>();

        var existing = preferences.Find(p => p != null && p.trait == trait);
        if (existing != null)
        {
            existing.preference = type;
        }
        else
        {
            preferences.Add(new PetPreference(trait, type));
        }
    }
}