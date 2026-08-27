using UnityEngine;

public class PetAnimation : MonoBehaviour
{
    [SerializeField] private Animator petAnimator;
    [SerializeField] private SpriteRenderer petRenderer;
 
    public void FlipSprite(bool isFlip)
    {
        petRenderer.flipX = isFlip;
    }
}
