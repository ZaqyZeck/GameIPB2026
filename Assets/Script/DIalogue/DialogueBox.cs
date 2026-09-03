using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject boxRoot;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] Image iconImage;
    [SerializeField] Button nextButton;
    [SerializeField] Button skipClickArea; 

    [Header("Settings")]
    [SerializeField] float charsPerSecond = 30f;

    List<DialoguePage> pages = new List<DialoguePage>();
    int currentPageIndex;
    Coroutine typingCoroutine;
    bool isTyping;
    Action onDialogueComplete;

    void Awake()
    {
        nextButton.onClick.AddListener(OnNextClicked);
        skipClickArea.onClick.AddListener(OnBoxClicked);
        Hide();
    }

    public void Show(List<DialoguePage> newPages, Action onComplete = null)
    {
        pages = newPages;
        currentPageIndex = 0;
        onDialogueComplete = onComplete;
        boxRoot.SetActive(true);
        ShowPage(currentPageIndex);
    }

    public void Hide()
    {
        boxRoot.SetActive(false);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        isTyping = false;
    }

    void ShowPage(int index)
    {
        if (index < 0 || index >= pages.Count)
        {
            Hide();
            onDialogueComplete?.Invoke();
            return;
        }

        DialoguePage page = pages[index];
        iconImage.gameObject.SetActive(page.icon != null);
        iconImage.sprite = page.icon;
        nextButton.gameObject.SetActive(false);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(page.text));
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
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

    void FinishTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        isTyping = false;
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        nextButton.gameObject.SetActive(true);
    }

    void OnBoxClicked()
    {
        if (isTyping) FinishTyping();
    }

    void OnNextClicked()
    {
        currentPageIndex++;
        ShowPage(currentPageIndex);
    }
}