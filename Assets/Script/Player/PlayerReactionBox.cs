using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerReactionBox : MonoBehaviour
{
    public static PlayerReactionBox Instance;

    [Header("References")]
    [SerializeField] GameObject boxRoot;
    [SerializeField] TextMeshProUGUI reactionText;

    [Header("Settings")]
    [SerializeField] float charsPerSecond = 30f;

    Coroutine activeRoutine;
    Action pendingCallback;

    public bool IsTyping { get; private set; }

    void Awake()
    {
        Instance = this;
        Hide();
    }

    /// <summary>
    /// Activates the box with empty text, without typing anything yet.
    /// Used at conversation start so both boxes appear together.
    /// </summary>
    public void ShowIdle()
    {
        boxRoot.SetActive(true);
        reactionText.text = string.Empty;
        reactionText.maxVisibleCharacters = 0;
    }

    /// <summary>
    /// Types out a new line into the box. Box stays active/visible afterward -
    /// caller decides when to Hide() it (e.g. only at end of conversation).
    /// </summary>
    public void ShowLine(string text, Action onTypingComplete = null)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        pendingCallback = onTypingComplete;
        activeRoutine = StartCoroutine(TypeText(text));
    }

    public void ShowRandomLine(string[] lines, Action onTypingComplete = null)
    {
        if (lines == null || lines.Length == 0) return;
        ShowLine(lines[UnityEngine.Random.Range(0, lines.Length)], onTypingComplete);
    }

    public void CompleteTyping()
    {
        if (!IsTyping) return;
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        FinishTyping();
    }

    IEnumerator TypeText(string fullText)
    {
        boxRoot.SetActive(true);
        IsTyping = true;

        reactionText.text = fullText;
        reactionText.maxVisibleCharacters = 0;
        reactionText.ForceMeshUpdate();

        if (reactionText.textInfo.characterCount == 0)
        {
            yield return null;
            reactionText.ForceMeshUpdate();
        }

        int totalChars = reactionText.textInfo.characterCount;
        int visible = 0;
        while (visible < totalChars)
        {
            visible++;
            reactionText.maxVisibleCharacters = visible;
            yield return new WaitForSeconds(1f / charsPerSecond);
        }

        FinishTyping();
    }

    void FinishTyping()
    {
        IsTyping = false;
        reactionText.maxVisibleCharacters = reactionText.textInfo.characterCount;

        Action callback = pendingCallback;
        pendingCallback = null;
        callback?.Invoke();
    }

    public void Hide()
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = null;
        IsTyping = false;
        pendingCallback = null;
        boxRoot.SetActive(false);
    }

    public void HideAfterDelay(float delay)
    {
        StartCoroutine(HideAfterDelayRoutine(delay));
    }

    IEnumerator HideAfterDelayRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
    }
}