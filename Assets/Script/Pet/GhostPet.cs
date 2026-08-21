using DG.Tweening;
using UnityEngine;

public class GhostPet : MonoBehaviour
{
    public int petId;
    public bool isOwnerArrived;
    public PetData petData;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] PetMovement movement;
    [SerializeField] PetBehaviorController behaviorController;

    public PetMovement Movement => movement;
    public PetBehaviorController BehaviorController => behaviorController;

    public void OwnerArrived()
    {
        isOwnerArrived = true;
    }

    public void Spawn(PetData newPetData)
    {
        petData = newPetData;
        behaviorController.Initialize(petData);

        SpawnAnimation();
    }

    public void Despawn()
    {
        DespawnAnimation();
    }
    void SpawnAnimation()
    {
        spriteRenderer.DOFade(1f, 1f);
    }

    void DespawnAnimation()
    {
        spriteRenderer.DOFade(0f, 1f).OnComplete(() => DestroySelf());
        //DestroySelf();
    }
    void DestroySelf()
    {
        Destroy(gameObject);
    }
}