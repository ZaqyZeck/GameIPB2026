using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class PetAnimation : MonoBehaviour
{
    [SerializeField] private Animator petAnimator;
    [SerializeField] private SpriteRenderer petRenderer;
    [SerializeField] private SpriteResolver petResolver;
    [SerializeField] private SpriteLibrary petSpriteLibrary;

    [SerializeField] private List<PetTypeLibraryEntry> spriteLibrariesByType = new();

    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int IsSittingHash = Animator.StringToHash("isSitting");
    private static readonly int ActionIdHash = Animator.StringToHash("ActionID");

    public void FlipSprite(bool isFlip)
{
    petRenderer.flipX = isFlip;
}

public bool IsFacingPositiveX => petRenderer != null && petRenderer.flipX;

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
        petAnimator.SetBool(IsWalkingHash, false);
        petAnimator.SetBool(IsSittingHash, false);
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

    public void SetSpriteLibraryForType(PetType type)
    {
        if (petSpriteLibrary == null) return;

        foreach (PetTypeLibraryEntry entry in spriteLibrariesByType)
        {
            if (entry.petType == type)
            {
                petSpriteLibrary.spriteLibraryAsset = entry.spriteLibraryAsset;
                return;
            }
        }

        Debug.LogWarning($"[PetAnimation] No SpriteLibraryAsset assigned for type {type} on {name}");
    }
}

[Serializable]
public class PetTypeLibraryEntry
{
    public PetType petType;
    public SpriteLibraryAsset spriteLibraryAsset;
}