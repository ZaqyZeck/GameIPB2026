using DG.Tweening;
using UnityEngine;

public class Owner : MonoBehaviour
{
    [SerializeField] string ownerName;
    //[SerializeField] int petId;
    [SerializeField] float patienceAmount = 60f;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Collider2D interactCollider;

    public bool isInLine;
    GhostPet currentPet;
    OwnerData currentOwnerData;

    public void Spawn(GhostPet wantedPet, OwnerData newOwnerData)
    {
        if (isInLine) return;

        currentPet = wantedPet;
        currentOwnerData = newOwnerData;
        ownerName = currentOwnerData.ownerName;

        SpawnAnimation();
    }
    public void Despawn()
    {
        if (!isInLine) return;

        currentPet = null;
        currentOwnerData = null;
        ownerName = null;

        DespawnAnimation();
    }
    void SpawnAnimation()
    {
        if (isInLine) return;
        spriteRenderer.DOFade(1f, 1f);
    }
    void DespawnAnimation()
    {
        if (!isInLine) return;
        spriteRenderer.DOFade(0f, 1f);
    }

    public OwnerData GetOwnerData()
    {
        return currentOwnerData;
    }
}
