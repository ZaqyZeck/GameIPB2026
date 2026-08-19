using System.Linq;
using UnityEngine;

public class OwnerManager : MonoBehaviour
{
    public static OwnerManager Instance;
    [SerializeField] Owner[] owners = new Owner[3];
    [SerializeField] OwnerProfileSO ownerDatas;

    float spawnTimer;
    [SerializeField] float minSpawnTime = 10f;
    [SerializeField] float maxSpawnTime = 15f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (spawnTimer >= 0f) spawnTimer -= Time.deltaTime;
        else
        {
            spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);
            RandomSpawnOwner();
        }
    }
    
    void RandomSpawnOwner()
    {
        Owner[] availableCustomers = owners.Where(customer => !customer.isInLine).ToArray();

        Owner owner = availableCustomers[Random.Range(0, availableCustomers.Length)];
        OwnerData ownerData = ownerData = ownerDatas.OwnerDatas[Random.Range(0, ownerDatas.OwnerDatas.Count)];
        while (!CheckDoubleOwnerData(ownerData)){
            ownerData = ownerDatas.OwnerDatas[Random.Range(0, ownerDatas.OwnerDatas.Count)];
        }

        Debug.Log($"Customer dan mendapatkan OwnerData: {ownerData.ownerName}");

        GhostPet ghostPet = PetManager.Instance.GetAvailableGhostPet();

        owner.Spawn(ghostPet, ownerData);
    }

    bool CheckDoubleOwnerData(OwnerData newOwnerData)
    {
        for (int i = 0; i < owners.Length; i++)
        {
            if (owners[i].GetOwnerData() == newOwnerData)
            {
                return true;
            }
        }

        return false;
    }
}
