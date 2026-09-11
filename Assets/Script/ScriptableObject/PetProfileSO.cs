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

    public PetData Clone()
    {
        return new PetData
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
    }
}