using DG.Tweening;
using TMPro;
using UnityEngine;

public class GhostPet : MonoBehaviour
{
    public int petId;
    public bool isOwnerArrived;
    public PetData petData;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] PetMovement movement;
    [SerializeField] PetBehaviorController behaviorController;
    [SerializeField] Vector3 targetSpawn;
    [SerializeField] TextMeshPro textPetId;

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

        textPetId.text = petId.ToString();

        SpawnAnimation();
    }

    public void Despawn()
    {
        DespawnAnimation();
    }
    void SpawnAnimation()
    {
        spriteRenderer.DOFade(1f, 1f).OnComplete(()=> { movement.MoveTo(targetSpawn); });
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