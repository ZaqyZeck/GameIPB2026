using DG.Tweening;
using UnityEngine;

public class Owner : MonoBehaviour
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
        isInLine = true;

        //Debug.Log(ownerName + " spawn");
        SpawnAnimation();
    }
    public void Despawn()
    {
        if (!isInLine) return;

        currentPet = null;
        currentOwnerData = null;
        ownerName = null;
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
}
