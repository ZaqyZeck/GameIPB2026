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

    public bool IsTyping { get; private set; }

    private void Awake()
    {
        Hide();
    }

    /// <summary>
    /// Shows this owner's line for the given page. onTypingComplete fires once
    /// the full line has finished typing out.
    /// </summary>
    public void ShowPage(DialoguePage page, Action onTypingComplete = null)
    {
        boxRoot.SetActive(true);
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(page.icon != null);
            iconImage.sprite = page.icon;
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

        Action callback = pendingCallback;
        pendingCallback = null;
        callback?.Invoke();
    }

    public void Hide()
    {
        boxRoot.SetActive(false);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        IsTyping = false;
        pendingCallback = null;
    }
}