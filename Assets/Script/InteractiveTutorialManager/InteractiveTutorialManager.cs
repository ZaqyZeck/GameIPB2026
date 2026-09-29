using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TutorialSystem
{
    public class InteractiveTutorialManager : MonoBehaviour
    {
        public static InteractiveTutorialManager Instance { get; private set; }

        public enum TutorialPhase { Typing, Delay, Ready }

        [SerializeField] private List<TutorialStep> steps = new List<TutorialStep>();

        [Header("Tutorial Icon")]
        [Tooltip("UI Image that displays the current step's mood/face icon (see TutorialStep.icon). Left disabled for None.")]
        [SerializeField] private Image tutorialIconImage;

        [SerializeField] private Sprite happyIconSprite;
        [SerializeField] private Sprite questionIconSprite;
        [SerializeField] private Sprite sadIconSprite;

        [Header("Clickable Front Layer")]
        [Tooltip("Optional Canvas layer where ClickButton targets are temporarily elevated above the dark overlay.")]
        [SerializeField] private Transform clickableFrontLayer;

        public event Action<TutorialStep> OnStepStarted;
        public event Action<TutorialStep> OnStepCompleted;
        public event Action OnTutorialCompleted;
        public event Action OnTutorialForceStopped;

        private int currentIndex = -1;
        public bool IsRunning { get; private set; }
        private TutorialPhase currentPhase = TutorialPhase.Ready;
        public TutorialPhase CurrentPhase => currentPhase;

        private Coroutine delayCoroutine;
        private Transform elevatedTarget;
        private Transform originalParent;
        private int originalSiblingIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (GameInputManager.Instance != null)
            {
                GameInputManager.Instance.OnPrimaryTap += HandleTapInput;
                GameInputManager.Instance.OnFastForward += HandleFastForwardInput;
            }
        }

        private void OnDestroy()
        {
            if (GameInputManager.Instance != null)
            {
                GameInputManager.Instance.OnPrimaryTap -= HandleTapInput;
                GameInputManager.Instance.OnFastForward -= HandleFastForwardInput;
            }
        }

        public void StartTutorial()
        {
            if (steps == null || steps.Count == 0)
            {
                Debug.LogWarning("[InteractiveTutorialManager] No tutorial steps assigned!");
                return;
            }

            IsRunning = true;
            currentIndex = -1;
            NextStep();
        }

        public void NextStep()
        {
            RestoreElevatedTarget();

            if (delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
                delayCoroutine = null;
            }

            currentIndex++;
            if (currentIndex >= steps.Count)
            {
                CompleteTutorial();
                return;
            }

            TutorialStep currentStep = steps[currentIndex];
            if (currentStep == null)
            {
                NextStep();
                return;
            }

            currentPhase = TutorialPhase.Typing;
            UpdateTutorialIcon(currentStep.icon);

            // Find matching TutorialTarget transforms in the scene
            List<Transform> targetTransforms = FindTargetTransforms(currentStep.targetKeys);

            // Elevate target if ClickButton
            if (currentStep.advanceType == StepAdvanceType.ClickButton && targetTransforms.Count > 0 && clickableFrontLayer != null)
            {
                ElevateTarget(targetTransforms[0]);
            }

            if (TutorialUIController.Instance != null)
            {
                TutorialUIController.Instance.gameObject.SetActive(true);
                TutorialUIController.Instance.ShowStep(
                    currentStep,
                    currentStep.message,
                    currentStep.instructionText,
                    targetTransforms,
                    null
                );
            }

            OnStepStarted?.Invoke(currentStep);

            StartCoroutine(WaitForTypingThenReady(currentStep));
        }

        private IEnumerator WaitForTypingThenReady(TutorialStep step)
        {
            while (TutorialUIController.Instance != null && TutorialUIController.Instance.IsTyping)
            {
                yield return null;
            }

            currentPhase = TutorialPhase.Ready;

            if (TutorialUIController.Instance != null)
            {
                TutorialUIController.Instance.ShowInstruction();
            }

            if (step.advanceType == StepAdvanceType.WaitForSeconds)
            {
                yield return new WaitForSeconds(step.autoAdvanceDelay);
                AdvanceIfCurrentStep(step);
            }
        }

        private void HandleTapInput(Vector2 screenPos)
        {
            if (!IsRunning || currentIndex < 0 || currentIndex >= steps.Count) return;

            TutorialStep step = steps[currentIndex];
            if (step == null) return;

            // If still typing, tapping skips typing first
            if (TutorialUIController.Instance != null && TutorialUIController.Instance.IsTyping)
            {
                TutorialUIController.Instance.SkipTyping();
                return;
            }

            if (step.advanceType == StepAdvanceType.AnyKey && currentPhase == TutorialPhase.Ready)
            {
                AdvanceIfCurrentStep(step);
            }
        }

        private void HandleFastForwardInput()
        {
            if (!IsRunning || currentIndex < 0 || currentIndex >= steps.Count) return;

            TutorialStep step = steps[currentIndex];
            if (step == null) return;

            if (TutorialUIController.Instance != null && TutorialUIController.Instance.IsTyping)
            {
                TutorialUIController.Instance.SkipTyping();
            }
            else if (step.advanceType == StepAdvanceType.AnyKey && currentPhase == TutorialPhase.Ready)
            {
                AdvanceIfCurrentStep(step);
            }
        }

        public void CompleteStep(string stepId)
        {
            if (!IsRunning || currentIndex < 0 || currentIndex >= steps.Count) return;

            TutorialStep step = steps[currentIndex];
            if (step != null && (string.IsNullOrEmpty(stepId) || step.stepId == stepId))
            {
                AdvanceIfCurrentStep(step);
            }
        }

        private void AdvanceIfCurrentStep(TutorialStep step)
        {
            if (!IsRunning || currentIndex < 0 || currentIndex >= steps.Count) return;
            if (steps[currentIndex] != step) return;

            OnStepCompleted?.Invoke(step);
            NextStep();
        }

        private void CompleteTutorial()
        {
            IsRunning = false;
            RestoreElevatedTarget();

            if (TutorialUIController.Instance != null)
            {
                TutorialUIController.Instance.HideAll();
            }

            UpdateTutorialIcon(TutorialIcon.None);
            OnTutorialCompleted?.Invoke();
        }

        public void ForceStopTutorial()
        {
            IsRunning = false;
            RestoreElevatedTarget();

            if (delayCoroutine != null)
            {
                StopCoroutine(delayCoroutine);
                delayCoroutine = null;
            }

            if (TutorialUIController.Instance != null)
            {
                TutorialUIController.Instance.HideAll();
            }

            UpdateTutorialIcon(TutorialIcon.None);
            OnTutorialForceStopped?.Invoke();
        }

        private List<Transform> FindTargetTransforms(List<string> keys)
        {
            List<Transform> results = new List<Transform>();
            if (keys == null || keys.Count == 0) return results;

            TutorialTarget[] allTargets = FindObjectsByType<TutorialTarget>(FindObjectsSortMode.None);
            foreach (string k in keys)
            {
                foreach (TutorialTarget t in allTargets)
                {
                    if (t != null && t.key == k)
                    {
                        results.Add(t.transform);
                        break;
                    }
                }
            }

            return results;
        }

        private void ElevateTarget(Transform target)
        {
            if (target == null || clickableFrontLayer == null) return;

            elevatedTarget = target;
            originalParent = target.parent;
            originalSiblingIndex = target.GetSiblingIndex();

            target.SetParent(clickableFrontLayer, true);
        }

        private void RestoreElevatedTarget()
        {
            if (elevatedTarget != null && originalParent != null)
            {
                elevatedTarget.SetParent(originalParent, true);
                elevatedTarget.SetSiblingIndex(originalSiblingIndex);
                elevatedTarget = null;
                originalParent = null;
            }
        }

        private void UpdateTutorialIcon(TutorialIcon icon)
        {
            if (tutorialIconImage == null) return;

            Sprite s = icon switch
            {
                TutorialIcon.Happy => happyIconSprite,
                TutorialIcon.Question => questionIconSprite,
                TutorialIcon.Sad => sadIconSprite,
                _ => null
            };

            tutorialIconImage.gameObject.SetActive(s != null);
            tutorialIconImage.sprite = s;
        }
    }
}