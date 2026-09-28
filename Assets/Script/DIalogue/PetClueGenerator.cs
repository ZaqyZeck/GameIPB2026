using System.Collections.Generic;
using UnityEngine;

public static class PetClueGenerator
{
    /// <summary>
    /// Generates the list of dialogue pages that the owner speaks to describe their lost pet.
    /// Supports both new data-driven PetPreferences (likes/dislikes) and fallback to legacy traits.
    /// </summary>
    public static List<DialoguePage> GenerateDialoguePages(Pet pet, Owner owner, TraitDatabaseSO traitDatabase = null)
    {
        List<DialoguePage> pages = new List<DialoguePage>();
        if (pet == null || pet.petData == null) return pages;

        if (traitDatabase == null && PetManager.Instance != null)
        {
            traitDatabase = PetManager.Instance.TraitDatabase;
        }

        PetData petData = pet.petData;

        // Page 1: Greeting
        string speciesLabel = petData.species == PetSpecies.Dog ? "dog" : "cat";
        pages.Add(new DialoguePage
        {
            text = $"Have you seen my {speciesLabel}?",
            icon = null,
            isDislike = false
        });

        // Check if pet has data-driven preferences
        bool hasPreferences = petData.preferences != null && petData.preferences.Count > 0;

        if (hasPreferences)
        {
            foreach (var pref in petData.preferences)
            {
                if (pref == null || pref.trait == null) continue;

                string text = pref.preference == PreferenceType.Dislike
                    ? pref.trait.GetRandomDislikeDialogue()
                    : pref.trait.GetRandomLikeDialogue();

                Sprite icon = pref.trait.GetIcon(pref.preference);

                // Fallback to PetIconDatabase if trait asset doesn't have an icon assigned
                if (icon == null && PetIconDatabase.Instance != null)
                {
                    if (pref.trait.actionType != ActionTrait.None)
                        icon = PetIconDatabase.Instance.GetActionIcon(pref.trait.actionType);
                    else if (pref.trait.habitType != HabitTrait.None)
                        icon = PetIconDatabase.Instance.GetHabitIcon(petData.petType, pref.trait.habitType);
                }

                pages.Add(new DialoguePage
                {
                    text = text,
                    icon = icon,
                    isDislike = (pref.preference == PreferenceType.Dislike)
                });
            }
        }
        else
        {
            // Legacy Fallback: Look up from TraitDatabase or Behavior Describables
            TraitSO actionTraitSO = traitDatabase != null ? traitDatabase.GetTraitByAction(petData.hiddenAction) : null;
            TraitSO habitTraitSO = traitDatabase != null ? traitDatabase.GetTraitByHabit(petData.hiddenHabit) : null;

            // Action Clue
            string actionText = actionTraitSO != null
                ? actionTraitSO.GetRandomLikeDialogue()
                : "Hmm, not sure what it likes to play with.";

            Sprite actionIcon = actionTraitSO != null ? actionTraitSO.GetIcon(PreferenceType.Like) : null;
            if (actionIcon == null && PetIconDatabase.Instance != null)
                actionIcon = PetIconDatabase.Instance.GetActionIcon(petData.hiddenAction);

            pages.Add(new DialoguePage
            {
                text = actionText,
                icon = actionIcon,
                isDislike = false
            });

            // Habit Clue
            string habitText = habitTraitSO != null
                ? habitTraitSO.GetRandomLikeDialogue()
                : "Hmm, not sure what it likes to do.";

            Sprite habitIcon = habitTraitSO != null ? habitTraitSO.GetIcon(PreferenceType.Like) : null;
            if (habitIcon == null && PetIconDatabase.Instance != null)
                habitIcon = PetIconDatabase.Instance.GetHabitIcon(petData.petType, petData.hiddenHabit);

            pages.Add(new DialoguePage
            {
                text = habitText,
                icon = habitIcon,
                isDislike = false
            });
        }

        // Appearance / Breed Clue
        Sprite typeIcon = PetIconDatabase.Instance != null ? PetIconDatabase.Instance.GetTypeIcon(petData.petType) : null;
        pages.Add(new DialoguePage
        {
            text = "Here's how it looks.",
            icon = typeIcon,
            isDislike = false
        });

        return pages;
    }
}
