using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PetManager : MonoBehaviour
{
    public static PetManager Instance;

    [SerializeField] private PetProfileSO petDatas;
    [SerializeField] private TraitDatabaseSO traitDatabase;
    [SerializeField] private DifficultyProfileSO difficultyProfile;
    [SerializeField] private bool rollDislikes = true;
    [SerializeField] private GameObject[] petPrefabs;
    [SerializeField] private List<Pet> ghostPets = new();
    //[SerializeField] private List<PetData> petsAtDoor = new();
    [SerializeField] private Collider2D barrierCollider;
    [SerializeField] private int maxPet;
    //[SerializeField] private int maxPetAtDoor = 3;
    [SerializeField] float minSpawnTime = 10f;
    [SerializeField] float maxSpawnTime = 15f;

    [SerializeField] float spawnTimer;
    [SerializeField] Vector3 spawnPosition;
    [SerializeField] Shader petShader;

    public TraitDatabaseSO TraitDatabase => traitDatabase;
    public DifficultyProfileSO DifficultyProfile
    {
        get => difficultyProfile;
        set => difficultyProfile = value;
    }
    public bool isPetAvailable;
    public bool isPetsAtDoor;

    //public int PetAtDoorCount => petsAtDoor.Count;

    private int petIdCounter = 0;

    private void Awake()
    {
        Instance = this;
        if (traitDatabase != null)
        {
            traitDatabase.InitializeLookup();
        }
        spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);
    }

    private void Update()
    {
        if (!LevelManager.Instance.IsPlaying) return;
        if (spawnTimer > 0f)
        {
            if (CanAddPetAtDoor())
            {
                spawnTimer -= Time.deltaTime;
            }
        }
        else
        {
            spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);
            AddPetAtDoor();
        }
    }

    private void AddPetAtDoor()
    {
        if (!CanAddPetAtDoor())
        {
            return;
        }

        PetData template = GetAvailablePetData();

        if (template == null)
        {
            Debug.Log("Semua PetData sedang digunakan.");
            return;
        }

        PetData petData = template.Clone();

        SpawnPetAtDoor(petData);
        isPetsAtDoor = true;
    }

    public void DoorOpen()
    {
        foreach(Pet pet in ghostPets)
        {
            if (pet.isEnteredDoor) continue;
            pet.EnterDoor();
        }
        isPetsAtDoor = false;
    }

    private void SpawnPetAtDoor(PetData petData)
    {
        if (petData == null) return;

        if (!TryRollUniqueTraits(petData))
        {
            Debug.Log("Gagal mendapatkan kombinasi trait (warna/habit/action) yang unik.");
            return;
        }

        Debug.Log($"Pet mendapatkan PetData: {petData.petName}");

        Pet newPet = Instantiate(petPrefabs[0], spawnPosition, Quaternion.identity).GetComponent<Pet>();

        petIdCounter++;
        newPet.petId = petIdCounter;

        AddPetToList(newPet);
        newPet.SpawnAtDoor(petData);

        if (petShader != null)
        {
            Material petMaterial = new Material(petShader);
            petMaterial.SetColor("_Color", petData.specialColor);
            newPet.SetMaterial(petMaterial);
        }
        if(GameManager.Instance != null) {
        GameManager.Instance.PlayAudio(GameManager.Instance.lonceng);
        }
        GameEventBus.OnPetSpawned?.Invoke(petData);
    }

    public void DespawnPet(Pet ghostPet)
    {
        RemovePetFromList(ghostPet);
        ghostPet.Despawn();
    }

    public Vector3 GetRandomPosition()
    {
        Bounds bounds = barrierCollider.bounds;

        for (int i = 0; i < 10; i++)
        {
            Vector2 randomPoint = new Vector2(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y));

            if (barrierCollider.OverlapPoint(randomPoint))
            {
                return new Vector3(randomPoint.x, randomPoint.y, transform.position.z);
            }
        }

        return barrierCollider.bounds.center;
    }

    private PetData GetAvailablePetData()
    {
        if (petDatas == null || petDatas.petDatas == null || petDatas.petDatas.Count == 0)
            return null;

        // Fallback: If no difficulty profile is assigned, use the original uniform random behavior
        if (difficultyProfile == null)
        {
            return petDatas.petDatas[Random.Range(0, petDatas.petDatas.Count)];
        }

        // 1. Collect PetTypes of pets currently active in the room
        List<PetType> activeTypesInRoom = ghostPets
            .Where(p => p != null && p.petData != null)
            .Select(p => p.petData.petType)
            .ToList();

        // 2. Decide if we should spawn a duplicate PetType based on difficulty chance
        bool shouldSpawnDuplicate = activeTypesInRoom.Count > 0 && Random.value < difficultyProfile.sameTypeSpawnChance;

        if (shouldSpawnDuplicate)
        {
            // Find active PetTypes that have not reached the max duplicate cap yet
            var eligibleDuplicateTypes = activeTypesInRoom
                .GroupBy(t => t)
                .Where(g => g.Count() < difficultyProfile.maxSameTypeInRoom)
                .Select(g => g.Key)
                .ToList();

            if (eligibleDuplicateTypes.Count > 0)
            {
                PetType targetType = eligibleDuplicateTypes[Random.Range(0, eligibleDuplicateTypes.Count)];
                var matchingTemplates = petDatas.petDatas
                    .Where(d => d.petType == targetType)
                    .ToList();

                if (matchingTemplates.Count > 0)
                {
                    return matchingTemplates[Random.Range(0, matchingTemplates.Count)];
                }
            }
        }

        // 3. If duplicate roll did not trigger, and forceUniqueOnFail is enabled:
        if (difficultyProfile.forceUniqueOnFail && activeTypesInRoom.Count > 0)
        {
            var distinctTemplates = petDatas.petDatas
                .Where(d => !activeTypesInRoom.Contains(d.petType))
                .ToList();

            if (distinctTemplates.Count > 0)
            {
                return distinctTemplates[Random.Range(0, distinctTemplates.Count)];
            }
        }

        // Fallback: standard uniform random
        return petDatas.petDatas[Random.Range(0, petDatas.petDatas.Count)];
    }

    public Pet GetAvailableGhostPet()
    {
        List<Pet> availablePets = new();

        foreach (Pet ghostPet in ghostPets)
        {
            if (!ghostPet.isOwnerArrived)
            {
                availablePets.Add(ghostPet);
            }
        }

        if (availablePets.Count == 0)
        {
            return null;
        }

        return availablePets[Random.Range(0, availablePets.Count)];
    }

    public void AddPetToList(Pet ghostPet)
    {
        ghostPets.Add(ghostPet);
    }

    public void RemovePetFromList(Pet ghostPet)
    {
        ghostPets.Remove(ghostPet);
    }

    public bool CheckPetWithoutOwner()
    {
        foreach (Pet pet in ghostPets)
        {
            if (!pet.isOwnerArrived) return true;
        }

        return false;
    }

    public bool CheckDoublePetData(PetData newPetData)
    {
        foreach (Pet ghostPet in ghostPets)
        {
            if (ghostPet.petData != null && ghostPet.petData.petName == newPetData.petName)
                return true;
        }

        return false;
    }

    private const int MaxTraitRollAttempts = 30;

    private bool TryRollUniqueTraits(PetData petData)
    {
        Color[] colorPool = (petDatas != null && petDatas.colorPool != null && petDatas.colorPool.Length > 0)
            ? petDatas.colorPool
            : null;

        bool shouldRollDislikes = difficultyProfile != null ? difficultyProfile.rollDislikes : rollDislikes;

        for (int attempt = 0; attempt < MaxTraitRollAttempts; attempt++)
        {
            Color rolledColor = colorPool != null
                ? colorPool[Random.Range(0, colorPool.Length)]
                : petData.specialColor;

            HabitTrait rolledHabit = RandomEnumValue<HabitTrait>();
            ActionTrait rolledAction = RandomEnumValue<ActionTrait>();

            if (!IsTraitComboInUse(petData.petType, rolledColor, rolledHabit, rolledAction))
            {
                petData.specialColor = rolledColor;
                petData.hiddenHabit = rolledHabit;
                petData.hiddenAction = rolledAction;
                petData.ownerSprite = RollOwnerSprite();

                petData.preferences.Clear();
                if (traitDatabase != null)
                {
                    TraitSO likeActionSO = traitDatabase.GetTraitByAction(rolledAction);
                    if (likeActionSO != null) petData.SetPreference(likeActionSO, PreferenceType.Like);

                    TraitSO likeHabitSO = traitDatabase.GetTraitByHabit(rolledHabit);
                    if (likeHabitSO != null) petData.SetPreference(likeHabitSO, PreferenceType.Like);

                    if (shouldRollDislikes)
                    {
                        ActionTrait dislikeAction = RollDifferentAction(rolledAction);
                        if (dislikeAction != ActionTrait.None)
                        {
                            TraitSO dislikeActionSO = traitDatabase.GetTraitByAction(dislikeAction);
                            if (dislikeActionSO != null) petData.SetPreference(dislikeActionSO, PreferenceType.Dislike);
                        }
                    }
                }

                return true;
            }
        }

        Debug.LogError("gak ketemu trait yang cocok");
        return false;
    }

    private ActionTrait RollDifferentAction(ActionTrait preferredAction)
    {
        ActionTrait[] allActions = new[] { ActionTrait.Football, ActionTrait.MiceToy, ActionTrait.CatToy };
        List<ActionTrait> candidates = new List<ActionTrait>();
        foreach (var act in allActions)
        {
            if (act != preferredAction) candidates.Add(act);
        }
        if (candidates.Count == 0) return ActionTrait.None;
        return candidates[Random.Range(0, candidates.Count)];
    }

    private Sprite RollOwnerSprite()
    {
        if (petDatas == null || petDatas.ownerSpritePool == null || petDatas.ownerSpritePool.Length == 0)
            return null;

        return petDatas.ownerSpritePool[Random.Range(0, petDatas.ownerSpritePool.Length)];
    }

    private static T RandomEnumValue<T>() where T : Enum
    {
        T[] values = (T[])Enum.GetValues(typeof(T));
        return values[Random.Range(1, values.Length)];
    }

    private bool IsTraitComboInUse(PetType petType, Color color, HabitTrait habit, ActionTrait action)
    {
        foreach (Pet ghostPet in ghostPets)
        {
            PetData other = ghostPet.petData;

            if (other == null) continue;

            // If same breed/type, prevent identical habit + action so players can always differentiate them
            if (other.petType == petType && other.hiddenHabit == habit && other.hiddenAction == action)
            {
                return true;
            }

            if (other.hiddenHabit == habit &&
                other.hiddenAction == action &&
                ColorsApproximatelyEqual(other.specialColor, color))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ColorsApproximatelyEqual(Color a, Color b)
    {
        const float eps = 0.001f;

        return Mathf.Abs(a.r - b.r) < eps &&
               Mathf.Abs(a.g - b.g) < eps &&
               Mathf.Abs(a.b - b.b) < eps &&
               Mathf.Abs(a.a - b.a) < eps;
    }

    public bool CanAddPetAtDoor()
    {
        int limit = (difficultyProfile != null && difficultyProfile.maxActivePets > 0)
            ? difficultyProfile.maxActivePets
            : (maxPet > 0 ? maxPet : 4);

        if (ghostPets.Count >= limit) return false;

        if (petDatas == null || petDatas.petDatas == null || petDatas.petDatas.Count == 0)
            return false;

        return true;
    }

    public List<Pet> GetPetsWithAction(ActionTrait trait)
    {
        List<Pet> result = new();

        foreach (Pet pet in ghostPets)
        {
            if (pet.BehaviorController.HasHiddenAction(trait))
                result.Add(pet);
        }

        return result;
    }

    public List<Pet> GetPetsWithHabit(HabitTrait trait)
    {
        List<Pet> result = new();

        foreach (Pet pet in ghostPets)
        {
            if (pet.BehaviorController.HasHiddenHabit(trait))
                result.Add(pet);
        }

        return result;
    }
}