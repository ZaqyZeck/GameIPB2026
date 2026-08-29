using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PetManager : MonoBehaviour
{
    public static PetManager Instance;

    [SerializeField] private PetProfileSO petDatas;
    [SerializeField] private GameObject[] petPrefabs;
    [SerializeField] private List<Pet> ghostPets = new();
    [SerializeField] private Collider2D barrierCollider;
    [SerializeField] private int maxPet;
    [SerializeField] float minSpawnTime = 10f;
    [SerializeField] float maxSpawnTime = 15f;

    [SerializeField] float spawnTimer;
    [SerializeField] Vector3 spawnPosition;
    public bool isPetAvailable;

    private int petIdCounter = 0;
    private void Awake()
    {
        Instance = this;
        spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);
    }

    private void Update()
    {
        if(spawnTimer > 0 )
        {
            if (CanSpawnPet())
            {
                spawnTimer -= Time.deltaTime;
            }
            
        }
        else
        {
            spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);
            SpawnPet();
        }
    }

    void SpawnPet()
    {
        if (!CanSpawnPet())
        {
            return;
        }

        PetData template = GetAvailablePetData();

        if (template == null)
        {
            Debug.Log("Semua PetData sedang digunakan.");
            return;
        }

        // Clone so we never mutate the shared PetProfileSO asset data.
        PetData petData = template.Clone();

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
        newPet.Spawn(petData);
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

        List<PetData> availablePetDatas = petDatas.petDatas.Where(petData => !CheckDoublePetData(petData)).ToList();

        if (availablePetDatas.Count == 0)
            return null;

        return availablePetDatas[Random.Range(0, availablePetDatas.Count)];
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
            // Compare by name rather than reference: spawned pets hold a
            // Clone() of the template, not the original PetProfileSO entry.
            if (ghostPet.petData != null && ghostPet.petData.petName == newPetData.petName)
                return true;
        }
        return false;
    }

    private const int MaxTraitRollAttempts = 30;

    // Rolls a random color (from petDatas.colorPool), HabitTrait and ActionTrait
    // onto petData, retrying until the resulting combo doesn't match any
    // currently-spawned pet. Returns false if it couldn't find a free combo
    // within MaxTraitRollAttempts (e.g. pool exhausted).
    private bool TryRollUniqueTraits(PetData petData)
    {
        Color[] colorPool = (petDatas != null && petDatas.colorPool != null && petDatas.colorPool.Length > 0)
            ? petDatas.colorPool
            : null;

        for (int attempt = 0; attempt < MaxTraitRollAttempts; attempt++)
        {
            Color rolledColor = colorPool != null
                ? colorPool[Random.Range(0, colorPool.Length)]
                : petData.specialColor;

            HabitTrait rolledHabit = RandomEnumValue<HabitTrait>();
            ActionTrait rolledAction = RandomEnumValue<ActionTrait>();

            if (!IsTraitComboInUse(rolledColor, rolledHabit, rolledAction))
            {
                petData.specialColor = rolledColor;
                petData.hiddenHabit = rolledHabit;
                petData.hiddenAction = rolledAction;
                return true;
            }
        }

        return false;
    }

    private static T RandomEnumValue<T>() where T : Enum
    {
        T[] values = (T[])Enum.GetValues(typeof(T));
        return values[Random.Range(0, values.Length)];
    }

    private bool IsTraitComboInUse(Color color, HabitTrait habit, ActionTrait action)
    {
        foreach (Pet ghostPet in ghostPets)
        {
            PetData other = ghostPet.petData;
            if (other == null) continue;

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

    public bool CanSpawnPet()
    {
        if (ghostPets.Count >= maxPet)
            return false;

        if (petDatas == null || petDatas.petDatas == null || petDatas.petDatas.Count == 0)
            return false;

        foreach (PetData petData in petDatas.petDatas)
        {
            if (!CheckDoublePetData(petData))
                return true;
        }

        return false;
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
}