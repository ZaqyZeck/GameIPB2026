using DG.Tweening;
using TMPro;
using UnityEngine;

public class Owner : Interactable
{
    [SerializeField] string ownerName;
    //[SerializeField] int petId;
    [SerializeField] float patienceAmount = 60f;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Collider2D interactCollider;
    [SerializeField] TextMeshPro textPetId;

    [SerializeField] GhostPet currentPet;
    OwnerData currentOwnerData;
    
    public bool isInLine;

    float patienceTimer;

    private void Awake()
    {
        patienceTimer = patienceAmount;
    }

    private void Update()
    {
        if(patienceTimer >= 0 && isInLine)
        {
            patienceTimer -= Time.deltaTime;
        }
        else
        {
            patienceTimer = patienceAmount;
            DespawnWithoutPet();
        }
    }

    public void Spawn(GhostPet wantedPet, OwnerData newOwnerData)
    {
        if (isInLine) return;

        currentPet = wantedPet;
        currentOwnerData = newOwnerData;
        ownerName = currentOwnerData.ownerName;

        textPetId.text = currentPet.petId.ToString();

        interactCollider.enabled = true;
        isInLine = true;

        //Debug.Log(ownerName + " spawn");
        SpawnAnimation();
    }
    public void DespawnWithoutPet()
    {
        if (!isInLine) return;

        //Despawn pet
        //PetManager.Instance.DespawnPet(currentPet);

        textPetId.text = null;

        currentPet = null;
        currentOwnerData = null;
        ownerName = null;
        interactCollider.enabled = false;
        isInLine = false;

        DespawnAnimation();
        OwnerManager.Instance.CheckLine();
    }

    public void DespawnWithPet()
    {
        if (!isInLine) return;

        //Despawn pet
        PetManager.Instance.DespawnPet(currentPet);

        textPetId.text = null;

        currentPet.isOwnerArrived = false;
        currentPet = null;
        currentOwnerData = null;
        ownerName = null;
        interactCollider.enabled = false;
        isInLine = false;

        DespawnAnimation();
        OwnerManager.Instance.CheckLine();
    }

    void SpawnAnimation()
    {
        //if (isInLine) return;
        spriteRenderer.DOFade(1f, 1f);
    }
    void DespawnAnimation()
    {
        //if (!isInLine) return;
        spriteRenderer.DOFade(0f, 1f);
    }
    public OwnerData GetOwnerData()
    {
        return currentOwnerData;
    }

    public bool GetPet(GhostPet ghostPet)
    {
        if(currentPet != ghostPet)
        {
            DespawnWithoutPet();
            return false;
        }
        Debug.Log("berhasil dapat pet");
        DespawnWithPet();
        return true;
    }

    //public override void Interact()
    //{
    //    GetPet(currentPet);
    //}
}
