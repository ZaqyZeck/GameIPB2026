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

    [Header("Settings")]
    [SerializeField] private float charsPerSecond = 30f;

    private Coroutine typingCoroutine;
    private Action pendingCallback;
    private Sprite pendingIcon;

    public bool IsTyping { get; private set; }

    private void Awake()
    {
        Hide();
    }

    public void ShowPage(DialoguePage page, Action onTypingComplete = null)
    {
        boxRoot.SetActive(true);
        Debug.Log($"[DialogueBox] boxRoot active state is now: {boxRoot.activeInHierarchy} (activeSelf: {boxRoot.activeSelf}), name={boxRoot.name}");

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(false);
        }

        pendingIcon = page.icon;

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

        while (visible < totalChars)
        {
            visible++;
            dialogueText.maxVisibleCharacters = visible;
            yield return new WaitForSeconds(delay);
        }

        FinishTyping();
    }

    private void FinishTyping()
    {
        IsTyping = false;
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;

        ShowIcon();

        Action callback = pendingCallback;
        pendingCallback = null;
        callback?.Invoke();
    }

    private void ShowIcon()
    {
        if (iconImage == null)
        {
            if (pendingIcon != null)
            {
                Debug.LogWarning($"[DialogueBox] Page wants an icon but iconImage is not assigned on {name}");
            }
            return;
        }

        iconImage.gameObject.SetActive(pendingIcon != null);
        iconImage.sprite = pendingIcon;
    }

    public void Hide()
    {
        boxRoot.SetActive(false);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        IsTyping = false;
        pendingCallback = null;
        pendingIcon = null;
        if (iconImage != null) iconImage.gameObject.SetActive(false);
    }
}