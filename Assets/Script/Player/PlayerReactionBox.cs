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
    [SerializeField] float defaultHideDelay = 2f;

    Coroutine activeRoutine;
    Action pendingCallback;

    public bool IsTyping { get; private set; }

    void Awake()
    {
        Instance = this;
        Hide();
    }

    /// <summary>
    /// Types out the given line. If hideDelay is negative, the default hide delay is used.
    /// Pass 0 to leave it up indefinitely until Hide() is called (used during conversations,
    /// so the controller decides exactly when it disappears).
    /// onTypingComplete fires as soon as typing finishes, before any auto-hide delay.
    /// </summary>
    public void ShowLine(string text, float hideDelay = -1f, Action onTypingComplete = null)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        pendingCallback = onTypingComplete;
        activeRoutine = StartCoroutine(TypeThenHide(text, hideDelay < 0f ? defaultHideDelay : hideDelay));
    }

    public void ShowRandomLine(string[] lines, float hideDelay = -1f, Action onTypingComplete = null)
    {
        if (lines == null || lines.Length == 0) return;
        ShowLine(lines[UnityEngine.Random.Range(0, lines.Length)], hideDelay, onTypingComplete);
    }

    public void CompleteTyping()
    {
        if (!IsTyping) return;
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        FinishTyping();
    }

    IEnumerator TypeThenHide(string fullText, float hideDelay)
    {
        boxRoot.SetActive(true);
        IsTyping = true;
        reactionText.text = fullText;
        reactionText.maxVisibleCharacters = 0;
        reactionText.ForceMeshUpdate();

        int totalChars = reactionText.textInfo.characterCount;
        int visible = 0;
        while (visible < totalChars)
        {
            visible++;
            reactionText.maxVisibleCharacters = visible;
            yield return new WaitForSeconds(1f / charsPerSecond);
        }

        FinishTyping();

        if (hideDelay > 0f)
        {
            yield return new WaitForSeconds(hideDelay);
            Hide();
        }
        // hideDelay == 0 means "stay up until told otherwise" - do nothing further.
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