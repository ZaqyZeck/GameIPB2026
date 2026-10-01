using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject boxRoot;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject dislikeIndicator;

    [Header("Text Constraint Settings")]
    [Tooltip("Maximum allowed width for the dialogue text before wrapping into multiple lines.")]
    [SerializeField] private float maxTextWidth = 360f;
    [Tooltip("Minimum height of the dialogue bubble to comfortably fit the icon with margins.")]
    [SerializeField] private float minBoxHeight = 65f;
    [Tooltip("If true, places icon on the left. If false, places icon on the right at the end of the text.")]
    [SerializeField] private bool iconOnLeft = false;

    [Header("Settings")]
    [SerializeField] private float charsPerSecond = 30f;
    [SerializeField] private int soundFrequency = 2;

    private Coroutine typingCoroutine;
    private Action pendingCallback;
    private Sprite pendingIcon;
    private bool pendingIsDislike;
    private LayoutElement textLayoutElement;
    private CanvasGroup iconCanvasGroup;

    public bool IsTyping { get; private set; }

    private void Awake()
    {
        Hide();
        SetupLayoutConstraints();
    }

    private void SetupLayoutConstraints()
    {
        if (boxRoot != null)
        {
            // Give enough top and bottom padding so the icon doesn't clip the top border or the bottom tail
            HorizontalLayoutGroup layoutGroup = boxRoot.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.padding.top = 10;
                layoutGroup.padding.bottom = 22;
                layoutGroup.padding.left = 16;
                layoutGroup.padding.right = 16;
                layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            }

            ContentSizeFitter fitter = boxRoot.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            LayoutElement boxLayout = boxRoot.GetComponent<LayoutElement>();
            if (boxLayout == null)
            {
                boxLayout = boxRoot.AddComponent<LayoutElement>();
            }
            boxLayout.minHeight = minBoxHeight;
        }

        // Configure child ordering (icon on left or right)
        if (iconImage != null && dialogueText != null)
        {
            if (iconOnLeft)
            {
                iconImage.transform.SetSiblingIndex(0);
                dialogueText.transform.SetSiblingIndex(1);
            }
            else
            {
                dialogueText.transform.SetSiblingIndex(0);
                iconImage.transform.SetSiblingIndex(1);
            }
        }

        if (dialogueText != null)
        {
            dialogueText.enableWordWrapping = true;
            dialogueText.overflowMode = TextOverflowModes.Overflow;

            textLayoutElement = dialogueText.GetComponent<LayoutElement>();
            if (textLayoutElement == null)
            {
                textLayoutElement = dialogueText.gameObject.AddComponent<LayoutElement>();
            }
            textLayoutElement.preferredWidth = maxTextWidth;
            textLayoutElement.flexibleWidth = 0;
        }

        EnsureIconCanvasGroup();
    }

    private void EnsureIconCanvasGroup()
    {
        if (iconImage != null && iconCanvasGroup == null)
        {
            iconCanvasGroup = iconImage.GetComponent<CanvasGroup>();
            if (iconCanvasGroup == null)
            {
                iconCanvasGroup = iconImage.gameObject.AddComponent<CanvasGroup>();
            }
            iconCanvasGroup.alpha = 0f;
            iconCanvasGroup.blocksRaycasts = false;
            iconCanvasGroup.interactable = false;
        }
    }

    public void ShowPage(DialoguePage page, Action onTypingComplete = null)
    {
        boxRoot.SetActive(true);

        pendingIcon = page.icon;
        pendingIsDislike = page.isDislike;

        EnsureIconCanvasGroup();

        // If the page has an icon, keep the GameObject active so HorizontalLayoutGroup
        // reserves its space immediately, but keep it invisible (alpha = 0) until typing completes.
        if (iconImage != null)
        {
            if (pendingIcon != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = pendingIcon;
                SetIconVisible(false);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        if (dislikeIndicator != null)
        {
            dislikeIndicator.SetActive(false);
        }

        // Dynamically fit bubble: compact for short text, bounded for long text
        if (dialogueText != null && textLayoutElement != null)
        {
            Vector2 preferredSize = dialogueText.GetPreferredValues(page.text, float.PositiveInfinity, float.PositiveInfinity);
            textLayoutElement.preferredWidth = Mathf.Min(preferredSize.x + 10f, maxTextWidth);
            LayoutRebuilder.ForceRebuildLayoutImmediate(boxRoot.GetComponent<RectTransform>());
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        pendingCallback = onTypingComplete;
        typingCoroutine = StartCoroutine(TypeText(page.text));
    }

    public void CompleteTyping()
    {
        if (!IsTyping) return;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        FinishTyping();
    }

    private IEnumerator TypeText(string fullText)
    {
        IsTyping = true;
        dialogueText.text = fullText;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalChars = dialogueText.textInfo.characterCount;
        float delay = 1f / charsPerSecond;
        int visible = 0;
        int charCount = 0;

        while (visible < totalChars)
        {
            visible++;
            dialogueText.maxVisibleCharacters = visible;

            if (soundFrequency > 0 && visible - 1 < dialogueText.textInfo.characterInfo.Length)
            {
                char c = dialogueText.textInfo.characterInfo[visible - 1].character;
                if (!char.IsWhiteSpace(c))
                {
                    charCount++;
                    if ((soundFrequency == 1 || charCount % soundFrequency == 1) && GameManager.Instance != null)
                    {
                        GameManager.Instance.PlayAudio(GameManager.Instance.dialogue);
                    }
                }
            }

            yield return new WaitForSeconds(delay);
        }

        FinishTyping();
    }

    private void FinishTyping()
    {
        IsTyping = false;
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;

        // Reveal the icon at the end of the text now that typing is done
        SetIconVisible(true);

        Action callback = pendingCallback;
        pendingCallback = null;
        callback?.Invoke();
    }

    private void SetIconVisible(bool visible)
    {
        if (iconImage == null)
        {
            if (pendingIcon != null)
            {
                Debug.LogWarning($"[DialogueBox] Page wants an icon but iconImage is not assigned on {name}");
            }
            return;
        }

        EnsureIconCanvasGroup();

        if (iconCanvasGroup != null)
        {
            iconCanvasGroup.alpha = visible ? 1f : 0f;
        }
        else
        {
            Color c = iconImage.color;
            c.a = visible ? 1f : 0f;
            iconImage.color = c;
        }

        if (dislikeIndicator != null)
        {
            dislikeIndicator.SetActive(visible && pendingIsDislike && pendingIcon != null);
        }
    }

    public void Hide()
    {
        boxRoot.SetActive(false);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        IsTyping = false;
        pendingCallback = null;
        pendingIcon = null;
        pendingIsDislike = false;
        SetIconVisible(false);
        if (iconImage != null) iconImage.gameObject.SetActive(false);
        if (dislikeIndicator != null) dislikeIndicator.SetActive(false);
    }
}