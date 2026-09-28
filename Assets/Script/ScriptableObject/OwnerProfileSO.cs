// OwnerProfileSO.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewOwnerProfile", menuName = "Owner Profile")]
public class OwnerProfileSO : ScriptableObject
{
    public List<OwnerData> OwnerDatas = new();
}

[Serializable]
public class OwnerData
{
    [Header("Majikan Info")]
    public string ownerName;
    public Sprite ownerSprite;
}