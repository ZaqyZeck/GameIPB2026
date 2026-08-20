using DG.Tweening;
using UnityEngine;

public class Owner : Interactable
{
    [SerializeField] string ownerName;
    //[SerializeField] int petId;
    [SerializeField] float patienceAmount = 60f;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Collider2D interactCollider;

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
            Despawn();
        }
    }

    public void Spawn(GhostPet wantedPet, OwnerData newOwnerData)
    {
        if (isInLine) return;

        currentPet = wantedPet;
        currentOwnerData = newOwnerData;
        ownerName = currentOwnerData.ownerName;
        interactCollider.enabled = true;
        isInLine = true;

        //Debug.Log(ownerName + " spawn");
        SpawnAnimation();
    }
    public void Despawn()
    {
        if (!isInLine) return;

        //Despawn pet
        PetManager.Instance.DespawnPet(currentPet);

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

    public void GetPet(GhostPet ghostPet)
    {
        if(currentPet != ghostPet) return;
        Debug.Log("berhasil dapat pet");
        Despawn();
    }

    //public override void Interact()
    //{
    //    GetPet(currentPet);
    //}
}
