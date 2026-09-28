using System;
using UnityEngine;

public enum TraitCategory
{
    Action,
    Habit,
    Visual,
    Other
}

public enum PreferenceType
{
    Like,
    Dislike,
    Neutral
}

[System.Serializable]
public class PetPreference
{
    public TraitSO trait;
    public PreferenceType preference = PreferenceType.Like;

    public PetPreference() { }

    public PetPreference(TraitSO trait, PreferenceType preference)
    {
        this.trait = trait;
        this.preference = preference;
    }
}

[CreateAssetMenu(fileName = "NewTrait", menuName = "Furgetful/Trait")]
public class TraitSO : ScriptableObject
{
    [Header("General Info")]
    public string traitId;
    public string displayName;
    public TraitCategory category = TraitCategory.Action;

    [Header("Legacy Enum Link (for backward compatibility)")]
    public ActionTrait actionType = ActionTrait.None;
    public HabitTrait habitType = HabitTrait.None;

    [Header("Icons")]
    [Tooltip("Icon shown when pet likes this trait / item.")]
    public Sprite iconLike;

    [Tooltip("Optional icon shown when pet dislikes this trait / item. If null, UI can fallback to iconLike or use a dislike overlay.")]
    public Sprite iconDislike;

    [Header("Dialogue Bank")]
    [Tooltip("Lines spoken by the owner when the pet LIKES this trait.")]
    [TextArea(2, 4)]
    public string[] likeDialogueLines;

    [Tooltip("Lines spoken by the owner when the pet DISLIKES this trait.")]
    [TextArea(2, 4)]
    public string[] dislikeDialogueLines;

    public Sprite GetIcon(PreferenceType preference)
    {
        if (preference == PreferenceType.Dislike && iconDislike != null)
        {
            return iconDislike;
        }
        return iconLike;
    }

    public string GetRandomLikeDialogue()
    {
        if (likeDialogueLines != null && likeDialogueLines.Length > 0)
        {
            return likeDialogueLines[UnityEngine.Random.Range(0, likeDialogueLines.Length)];
        }
        return !string.IsNullOrEmpty(displayName) ? $"It loves {displayName.ToLower()}." : "It loves this.";
    }

    public string GetRandomDislikeDialogue()
    {
        if (dislikeDialogueLines != null && dislikeDialogueLines.Length > 0)
        {
            return dislikeDialogueLines[UnityEngine.Random.Range(0, dislikeDialogueLines.Length)];
        }
        return !string.IsNullOrEmpty(displayName) ? $"It really dislikes {displayName.ToLower()}!" : "It doesn't like this!";
    }
}
