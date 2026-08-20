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
        else if(!isLineFull && PetManager.Instance.CheckPetAvailability())
        {
            spawnTimer -= Time.deltaTime;
        }
    }
    
    void RandomSpawnOwner()
    {
        Owner[] availableCustomers = owners.Where(owner => !owner.isInLine).ToArray();

        if(availableCustomers.Length == 0)
        {
            Debug.Log("LinePenuh1");
            return;
        }
        Owner owner = availableCustomers[Random.Range(0, availableCustomers.Length)];
        OwnerData ownerData = ownerData = ownerDatas.OwnerDatas[Random.Range(0, ownerDatas.OwnerDatas.Count)];
        while (CheckDoubleOwnerData(ownerData)){
            ownerData = ownerDatas.OwnerDatas[Random.Range(0, ownerDatas.OwnerDatas.Count)];
        }

        Debug.Log($"Customer dan mendapatkan OwnerData: {ownerData.ownerName}");

        GhostPet ghostPet = PetManager.Instance.GetAvailableGhostPet();
        if(ghostPet == null)
        {
            Debug.Log("no pet available");
            return;
        }
        ghostPet.OwnerArrived();

        owner.Spawn(ghostPet, ownerData);
        CheckLine();
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

    public void CheckLine()
    {
        isLineFull = owners.Where(owner => !owner.isInLine).ToArray().Length == 0;
    }
}
