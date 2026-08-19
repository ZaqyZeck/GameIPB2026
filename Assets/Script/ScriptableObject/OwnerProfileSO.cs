// OwnerProfileSO.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOwnerProfile", menuName = "Owner Profile")]
public class OwnerProfileSO : ScriptableObject
{
    public List<OwnerData> OwnerDatas = new();

    //[Header("Petunjuk Kenangan (Clues)")]
    //[Tooltip("The specific habit the owner remembers")]
    //public HabitTrait rememberedHabit;
    
    //[Tooltip("The specific action the owner remembers")]
    //public ActionTrait rememberedAction;

    //// A helper function to check if a pet is a perfect match
    //public bool CheckMatch(PetProfileSO petToCheck)
    //{
    //    return (petToCheck.hiddenHabit == rememberedHabit) && 
    //           (petToCheck.hiddenAction == rememberedAction);
    //}
}

[Serializable]
public class OwnerData
{
    [Header("Majikan Info")]
    public string ownerName;
    public Sprite ownerSprite;
}