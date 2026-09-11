using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReactionBox : MonoBehaviour
{
    public static PlayerReactionBox Instance;

    [Header("References")]
    [SerializeField] GameObject boxRoot;
    [SerializeField] TextMeshProUGUI reactionText;

    Image boxImage;

    [Header("Settings")]
    [SerializeField] float charsPerSecond = 30f;
    [SerializeField] float fadeOutDuration = 0.3f;

    Coroutine activeRoutine;
    Coroutine delayedHideRoutine;
    Coroutine fadeRoutine;
    Action pendingCallback;

    public bool IsTyping { get; private set; }

    void Awake()
    {
        Instance = this;
        boxImage = boxRoot.GetComponent<Image>();
        HideImmediate();
    }

    /// <summary>
    /// Activates the box with empty text, without typing anything yet.
    /// Used at conversation start so both boxes appear together.
    /// </summary>
    public void ShowIdle()
    {
        CancelDelayedHide();
        CancelFade();

        boxRoot.SetActive(true);
        SetAlpha(1f);
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

        CancelDelayedHide();
        CancelFade();

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
        SetAlpha(1f);
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
        CancelDelayedHide();

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = null;
        IsTyping = false;
        pendingCallback = null;

        if (boxRoot.activeSelf)
        {
            CancelFade();
            fadeRoutine = StartCoroutine(FadeOutRoutine());
        }
        else
        {
            boxRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Hides instantly with no fade. Used on startup, where there's nothing
    /// on screen yet to transition away from.
    /// </summary>
    void HideImmediate()
    {
        CancelDelayedHide();
        CancelFade();

        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = null;
        IsTyping = false;
        pendingCallback = null;

        boxRoot.SetActive(false);
        SetAlpha(1f);
    }

    IEnumerator FadeOutRoutine()
    {
        float startAlpha = reactionText.color.a;
        float t = 0f;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, 0f, t / fadeOutDuration));
            yield return null;
        }

        // Box (and buttons) disappear instantly here, same as before -
        // only the text and background image faded.
        boxRoot.SetActive(false);
        SetAlpha(1f); // reset so the box is fully visible next time it's shown
        fadeRoutine = null;
    }

    void SetAlpha(float alpha)
    {
        Color textColor = reactionText.color;
        textColor.a = alpha;
        reactionText.color = textColor;

        if (boxImage != null)
        {
            Color imageColor = boxImage.color;
            imageColor.a = alpha;
            boxImage.color = imageColor;
        }
    }

    void CancelFade()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }

    public void HideAfterDelay(float delay)
    {
        CancelDelayedHide();
        delayedHideRoutine = StartCoroutine(HideAfterDelayRoutine(delay));
    }

    void CancelDelayedHide()
    {
        if (delayedHideRoutine != null)
        {
            StopCoroutine(delayedHideRoutine);
            delayedHideRoutine = null;
        }
    }

    IEnumerator HideAfterDelayRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        delayedHideRoutine = null;
        Hide();
    }
}