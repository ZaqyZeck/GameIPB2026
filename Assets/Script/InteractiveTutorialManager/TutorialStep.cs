using System.Collections.Generic;
using UnityEngine;

namespace TutorialSystem
{
    public enum StepAdvanceType
    {
        AnyKey,            // Advances on tap or any key
        WaitForSignal,     // Completes when a signal is received via InteractiveTutorialManager.Instance.CompleteStep(stepId)
        WaitForSeconds,    // Auto-advances after autoAdvanceDelay
        ClickButton        // Completes when target button is clicked
    }

    public enum TutorialIcon
    {
        None,
        Happy,
        Question,
        Sad
    }

    [CreateAssetMenu(fileName = "NewTutorialStep", menuName = "Tutorial/Tutorial Step")]
    public class TutorialStep : ScriptableObject
    {
        public string stepId;

        [TextArea(2, 5)] public string title;
        [TextArea(3, 8)] public string message;

        [Tooltip("Face/mood icon shown for this step. None hides the icon.")]
        public TutorialIcon icon = TutorialIcon.None;

        [TextArea(1, 2)]
        public string instructionText = "Tap or press anywhere to continue";

        [Header("Highlight Settings")]
        public bool highlightTargets = true;
        public List<string> targetKeys = new List<string>();

        [Header("Dialogue Placement")]
        public Vector2 defaultDialoguePosition = Vector2.zero;
        public bool snapDialogueToFirstTarget = false;
        public Vector2 dialogueOffsetFromTarget = new Vector2(0f, 120f);

        [Header("Progression")]
        public StepAdvanceType advanceType = StepAdvanceType.AnyKey;
        public float autoAdvanceDelay = 2f;
        public bool pauseGameDuringStep = false;
    }
}