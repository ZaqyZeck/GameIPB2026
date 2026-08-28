using UnityEngine;
using UnityEngine.U2D.Animation;

public class PetAnimation : MonoBehaviour
{
    [SerializeField] private Animator petAnimator;
    [SerializeField] private SpriteRenderer petRenderer;
    [SerializeField] private SpriteResolver petResolver;

    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int IsSittingHash = Animator.StringToHash("isSitting");
    private static readonly int ActionIdHash = Animator.StringToHash("ActionID");

    public void FlipSprite(bool isFlip)
    {
        petRenderer.flipX = isFlip;
    }

    public void SetWalking(bool isWalking)
    {
        petAnimator.SetBool(IsWalkingHash, isWalking);

        if (isWalking)
        {
            petAnimator.SetBool(IsSittingHash, false);
            ResetAction();
        }
    }

    public void SetSitting(bool isSitting)
    {
        petAnimator.SetBool(IsSittingHash, isSitting);

        if (isSitting)
        {
            petAnimator.SetBool(IsWalkingHash, false);
            ResetAction();
        }
    }

    public void TriggerAction(int actionId)
    {
        SetWalking(false);
        SetSitting(false);
        petAnimator.SetInteger(ActionIdHash, actionId);
    }

    public void ResetAction()
    {
        petAnimator.SetInteger(ActionIdHash, 0);
    }

    public void SetSpriteCategory(string category, string label = "0")
    {
        if (petResolver == null) return;
        petResolver.SetCategoryAndLabel(category, label);
    }
}