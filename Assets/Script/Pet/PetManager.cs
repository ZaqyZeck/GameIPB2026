using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PetManager : MonoBehaviour
{
    public static PetManager Instance;

    [SerializeField] private PetProfileSO petDatas;
    [SerializeField] private GameObject[] petPrefabs;
    [SerializeField] private List<GhostPet> ghostPets = new();
    [SerializeField] private Collider2D barrierCollider;
    [SerializeField] private int maxPet;
    [SerializeField] float minSpawnTime = 10f;
    [SerializeField] float maxSpawnTime = 15f;

    [SerializeField]  float spawnTimer;
    public bool isPetAvailable;
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
        PetData petData = petData = petDatas.petDatas[Random.Range(0, petDatas.petDatas.Count)];
        while (CheckDoublePetData(petData))
        {
            petData = petDatas.petDatas[Random.Range(0, petDatas.petDatas.Count)];
        }

        Debug.Log($"Customer dan mendapatkan OwnerData: {petData.petName}");

        GhostPet newPet = Instantiate(petPrefabs[0], GetSpawnPoint(), new Quaternion()).GetComponent<GhostPet>();

        AddPetToList(newPet);
        newPet.Spawn(petData);
        GameEventBus.OnPetSpawned?.Invoke(petData);
    }

    public void DespawnPet(GhostPet ghostPet)
    {
        RemovePetFromList(ghostPet);
        ghostPet.Despawn();
    }

    private Vector3 GetSpawnPoint()
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

    public GhostPet GetAvailableGhostPet()
    {
        List<GhostPet> availablePets = new();

        foreach (GhostPet ghostPet in ghostPets)
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

    public void AddPetToList(GhostPet ghostPet)
    {
        ghostPets.Add(ghostPet);
    }
    public void RemovePetFromList(GhostPet ghostPet)
    {
        ghostPets.Remove(ghostPet);
    }

    public bool CheckPetWithoutOwner()
    {
        foreach (GhostPet pet in ghostPets)
        {
            if (!pet.isOwnerArrived) return true;
        }
        return false;
    }

    public bool CheckDoublePetData(PetData newPetData)
    {
        foreach (GhostPet ghostPet in ghostPets)
        {
            if (ghostPet.petData == newPetData) return true;
        }
        return false;
    }

    public bool CanSpawnPet()
    {
        if(ghostPets.Count() >= maxPet) return false;
        // mungkin mau tambahin pengecekan jumlah pet melebihi petData
        return true;
    }
}