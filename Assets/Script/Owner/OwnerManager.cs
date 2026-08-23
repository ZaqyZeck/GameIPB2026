using System.Linq;
using UnityEngine;

public class OwnerManager : MonoBehaviour
{
    public static OwnerManager Instance;
    [SerializeField] Owner[] owners = new Owner[3];
    [SerializeField] OwnerProfileSO ownerDatas;

    [SerializeField] float spawnTimer;
    [SerializeField] float minSpawnTime = 10f;
    [SerializeField] float maxSpawnTime = 15f;

    bool isLineFull ;

    private void Awake()
    {
        Instance = this;
        spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);
    }

    private void Update()
    {
        if(spawnTimer <= 0f)
        {
            spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);
            RandomSpawnOwner();
        }
        else if(!isLineFull && PetManager.Instance.CheckPetWithoutOwner())
        {
            spawnTimer -= Time.deltaTime;
        }
    }

    void RandomSpawnOwner()
    {
        Owner[] availableOwners = owners.Where(owner => !owner.isInLine).ToArray();

        if (availableOwners.Length == 0)
        {
            Debug.Log("LinePenuh1");
            return;
        }

        OwnerData ownerData = GetAvailableOwnerData();

        if (ownerData == null)
        {
            Debug.Log("Semua OwnerData sedang digunakan.");
            return;
        }

        Owner owner = availableOwners[Random.Range(0, availableOwners.Length)];

        Debug.Log($"Customer dan mendapatkan OwnerData: {ownerData.ownerName}");

        GhostPet ghostPet = PetManager.Instance.GetAvailableGhostPet();

        if (ghostPet == null)
        {
            Debug.Log("no pet available");
            return;
        }

        ghostPet.OwnerArrived();

        owner.Spawn(ghostPet, ownerData);
        CheckLine();
    }

    OwnerData GetAvailableOwnerData()
    {
        if (ownerDatas == null || ownerDatas.OwnerDatas == null || ownerDatas.OwnerDatas.Count == 0)
            return null;

        var availableOwnerDatas = ownerDatas.OwnerDatas
            .Where(ownerData => !CheckDoubleOwnerData(ownerData))
            .ToList();

        if (availableOwnerDatas.Count == 0)
            return null;

        return availableOwnerDatas[Random.Range(0, availableOwnerDatas.Count)];
    }

    bool CheckDoubleOwnerData(OwnerData newOwnerData)
    {
        return owners.Any(owner => owner.isInLine && owner.GetOwnerData() == newOwnerData);
    }

    public void CheckLine()
    {
        isLineFull = owners.Where(owner => !owner.isInLine).ToArray().Length == 0;
    }
}
