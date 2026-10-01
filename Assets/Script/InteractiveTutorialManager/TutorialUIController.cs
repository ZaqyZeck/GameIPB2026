using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using DG.Tweening;

namespace TutorialSystem
{
    public class TutorialUIController : MonoBehaviour
    {
        public static TutorialUIController Instance { get; private set; }

        [Header("Dialogue Box")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private RectTransform dialogueBox;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        
        [Tooltip("The smaller text indicating what button to press to continue.")]
        [SerializeField] private TMP_Text instructionText; 

        [Header("Typing Animation")]
        [Tooltip("How fast each character appears in seconds.")]
        [SerializeField] private float typingSpeed = 0.02f;

        [Header("Highlighting")]
        [Tooltip("Full-screen darkening overlay using the circular cutout shader. Drives the spotlight effect.")]
        [SerializeField] private TutorialSpotlight spotlight;

        [Header("Skip")]
        [Tooltip("Optional button that lets the player force-stop the tutorial entirely.")]
        [SerializeField] private Button skipButton;

        private bool isPanelOpen = false;
        
        public bool IsTyping { get; private set; }
        private Coroutine typingCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); 
                return;
            }
            
            Instance = this;
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (skipButton != null)
                skipButton.onClick.AddListener(HandleSkipButtonPressed);
        }

        private void OnDisable()
        {
            if (skipButton != null)
                skipButton.onClick.RemoveListener(HandleSkipButtonPressed);
        }

        private void HandleSkipButtonPressed()
        {
            if (InteractiveTutorialManager.Instance == null) return;
            if (!InteractiveTutorialManager.Instance.IsRunning) return;

            InteractiveTutorialManager.Instance.ForceStopTutorial();
        }

        /// <summary>
        /// keyTargets and pathTargets are drawn as two INDEPENDENT highlight groups rather than
        /// being merged into one:
        ///   - keyTargets (from TutorialStep.targetKeys) always get one individual circle each,
        ///     same as plain single-target highlighting always has.
        ///   - pathTargets (the DragToTile/KnockToTile endpoint + its tile-path proxies, built by
        ///     InteractiveTutorialManager.SetupDragToTileHighlight) always get ONE combined bounds
        ///     ring around all of them together, since together they represent a single path to
        ///     follow rather than several separate things to look at.
        /// A step can populate either list, both, or neither - whichever are non-empty get shown,
        /// simultaneously, in their own style.
        /// </summary>
        public void ShowStep(TutorialStep step, string formattedMain, string formattedInst, List<Transform> keyTargets, List<Transform> pathTargets)
        {
            if (titleText != null) titleText.text = step.title;
            
            // Setup the instruction text but hide it until the typing/delay phase is over
            if (instructionText != null) 
            {
                instructionText.text = formattedInst;
                instructionText.gameObject.SetActive(false);
            }

            // Animate dialogue popup if not already active
            if (!isPanelOpen)
            {
                panelRoot.SetActive(true);
                dialogueBox.localScale = Vector3.zero;
                dialogueBox.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
                isPanelOpen = true;
            }
            else
            {
                dialogueBox.DOKill();
                dialogueBox.localScale = Vector3.one; 
                dialogueBox.DOPunchScale(Vector3.one * 0.05f, 0.3f, 5, 1f).SetUpdate(true);
            }

            bool hasKeyTargets = keyTargets != null && keyTargets.Count > 0;
            bool hasPathTargets = pathTargets != null && pathTargets.Count > 0;

            Vector2 targetAnchoredPos;

            if (step.highlightTargets && (hasKeyTargets || hasPathTargets))
            {
                if (spotlight != null)
                {
                    spotlight.HighlightGroups(
                        hasKeyTargets ? keyTargets : null,
                        hasPathTargets ? pathTargets : null);
                }

                // Prefer keyTargets[0] as the dialogue-snap anchor - it's an authored UI/world
                // TutorialTarget and the more meaningful thing to snap the dialogue box next to.
                // Fall back to the first path target (the origin endpoint, per
                // SetupDragToTileHighlight's ordering) only if there are no keyTargets at all.
                Transform primaryTarget = hasKeyTargets ? keyTargets[0] : pathTargets[0];

                if (step.snapDialogueToFirstTarget && primaryTarget is RectTransform firstRect)
                {
                    Vector2 originalPos = dialogueBox.anchoredPosition;
                    
                    dialogueBox.position = firstRect.position;
                    dialogueBox.anchoredPosition += step.dialogueOffsetFromTarget;
                    
                    targetAnchoredPos = dialogueBox.anchoredPosition;
                    dialogueBox.anchoredPosition = originalPos;
                }
                else
                {
                    targetAnchoredPos = step.defaultDialoguePosition;
                }
            }
            else
            {
                if (spotlight != null) spotlight.HighlightGroups(null, null);
                targetAnchoredPos = step.defaultDialoguePosition;
            }

            // --- SCREEN CLAMPING LOGIC ---
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null && parentCanvas.isRootCanvas)
            {
                RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                float minX = (canvasRect.rect.width * -0.5f) + (dialogueBox.rect.width * dialogueBox.pivot.x);
                float maxX = (canvasRect.rect.width * 0.5f) - (dialogueBox.rect.width * (1f - dialogueBox.pivot.x));
                float minY = (canvasRect.rect.height * -0.5f) + (dialogueBox.rect.height * dialogueBox.pivot.y);
                float maxY = (canvasRect.rect.height * 0.5f) - (dialogueBox.rect.height * (1f - dialogueBox.pivot.y));

                targetAnchoredPos.x = Mathf.Clamp(targetAnchoredPos.x, minX, maxX);
                targetAnchoredPos.y = Mathf.Clamp(targetAnchoredPos.y, minY, maxY);
            }

            dialogueBox.DOAnchorPos(targetAnchoredPos, 0.4f).SetEase(Ease.InOutQuad).SetUpdate(true);

            // Start typing effect!
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeMessageRoutine(formattedMain));
        }

        private IEnumerator TypeMessageRoutine(string message)
        {
            IsTyping = true;
            if (messageText != null)
            {
                messageText.text = message;
                messageText.maxVisibleCharacters = 0;
                messageText.ForceMeshUpdate();

                int totalChars = messageText.textInfo.characterCount;
                int charCount = 0;
                for (int i = 0; i <= totalChars; i++)
                {
                    messageText.maxVisibleCharacters = i;
                    if (i > 0 && i - 1 < messageText.textInfo.characterInfo.Length)
                    {
                        char c = messageText.textInfo.characterInfo[i - 1].character;
                        if (!char.IsWhiteSpace(c))
                        {
                            charCount++;
                            if (charCount % 2 == 1 && GameManager.Instance != null)
                            {
                                GameManager.Instance.PlayAudio(GameManager.Instance.dialogue);
                            }
                        }
                    }
                    yield return new WaitForSecondsRealtime(typingSpeed);
                }
            }
            IsTyping = false;
        }

        public void SkipTyping()
        {
            if (!IsTyping) return;
            
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            
            if (messageText != null)
            {
                messageText.maxVisibleCharacters = 99999;
            }
            IsTyping = false;
        }

        public void ShowInstruction()
        {
            if (instructionText != null && !string.IsNullOrEmpty(instructionText.text))
            {
                instructionText.gameObject.SetActive(true);
            }
        }

        public void UpdateTexts(string mainMsg, string instMsg)
        {
            if (messageText != null && !IsTyping)
            {
                messageText.text = mainMsg;
                messageText.maxVisibleCharacters = 99999;
            }
            if (instructionText != null)
            {
                instructionText.text = instMsg;
            }
        }

        public void HideAll()
        {
            if (spotlight != null) spotlight.Hide();

            if (isPanelOpen)
            {
                dialogueBox.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
                {
                    panelRoot.SetActive(false);
                    isPanelOpen = false;
                });
            }
            else
            {
                panelRoot.SetActive(false);
            }
        }
    }
}