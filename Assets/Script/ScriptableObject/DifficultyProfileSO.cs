using UnityEngine;

[CreateAssetMenu(fileName = "NewDifficultyProfile", menuName = "Pet/Difficulty Profile")]
public class DifficultyProfileSO : ScriptableObject
{
    [Header("Spawn Clustering (Pet Types)")]
    [Range(0f, 1f)]
    [Tooltip("Probability (0.0 to 1.0) of spawning a PetType that already exists in the room.")]
    public float sameTypeSpawnChance = 0.5f;

    [Tooltip("Maximum number of pets of the exact same PetType allowed in the room simultaneously.")]
    public int maxSameTypeInRoom = 2;

    [Tooltip("If true and a duplicate roll does not occur, actively selects a PetType not currently in the room.")]
    public bool forceUniqueOnFail = true;

    [Header("Trait Rules")]
    [Tooltip("Whether pets rolled under this difficulty should roll dislikes.")]
    public bool rollDislikes = true;
}
