using UnityEngine;

public enum DifficultyTier
{
    Easy,
    Medium,
    Adaptive
}

[CreateAssetMenu(fileName = "NewDifficultyProfile", menuName = "Pet/Difficulty Profile")]
public class DifficultyProfileSO : ScriptableObject
{
    [Header("Difficulty Tier")]
    public DifficultyTier tier = DifficultyTier.Easy;

    [Header("Balancing Settings")]
    [Tooltip("Additional patience time added to NPC in seconds (+30s default).")]
    public float extraPatienceTime = 30f;

    [Tooltip("Maximum active pets in the room (4 for Easy, 6 for Medium).")]
    public int maxActivePets = 4;

    [Tooltip("Number of spirit stones needed to win (5 default).")]
    public int targetStones = 5;

    [Header("Spawn Clustering (Pet Types)")]
    [Range(0f, 1f)]
    [Tooltip("Probability (0.0 to 1.0) of spawning a PetType that already exists in the room.")]
    public float sameTypeSpawnChance = 0f;

    [Tooltip("Maximum number of pets of the exact same PetType allowed in the room simultaneously.")]
    public int maxSameTypeInRoom = 2;

    [Tooltip("If true and a duplicate roll does not occur, actively selects a PetType not currently in the room.")]
    public bool forceUniqueOnFail = true;

    [Header("Trait Rules")]
    [Tooltip("Whether pets rolled under this difficulty should roll dislikes.")]
    public bool rollDislikes = true;
}
