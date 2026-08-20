// PetProfileSO.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPetProfile", menuName = "Pet Profile")]
public class PetProfileSO : ScriptableObject
{
    public List<PetData> petDatas = new();
}

[Serializable]
public class PetData
{
    [Header("Identitas Arwah")]
    public string petName = "Unknown Soul";
    public Sprite petSprite; // Base appearance

    [Header("Ciri-Ciri Kasat Mata (Immediate)")]
    public VisualTrait visualTrait;
    public Color specialColor = Color.white;

    [Header("Ciri-Ciri Habits (Passive/Timer)")]
    public HabitTrait hiddenHabit;
    public float timeToRevealHabit = 15f;

    [Header("Ciri-Ciri Action (Interactive)")]
    public ActionTrait hiddenAction;
}