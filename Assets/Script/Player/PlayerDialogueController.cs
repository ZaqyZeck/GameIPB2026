using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDialogueController : MonoBehaviour
{
    public static PlayerDialogueController Instance;

    [Header("References")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipClickArea; // transparent, covers the boxes; click = skip current typing

    private DialogueBox activeOwnerBox;
    private List<DialoguePage> pages;
    private int currentPageIndex;
    private string[] advanceLines;
    private string[] farewellLines;

    private bool waitingForNext;

    private void Awake()
    {
        Instance = this;
        nextButton.onClick.AddListener(OnNextClicked);
        skipClickArea.onClick.AddListener(OnSkipClicked);
        nextButton.gameObject.SetActive(false);
    }

    public void StartConversation(DialogueBox ownerBox, List<DialoguePage> newPages, string[] newAdvanceLines, string[] newFarewellLines)
    {
        activeOwnerBox = ownerBox;
        pages = newPages;
        advanceLines = newAdvanceLines;
        farewellLines = newFarewellLines;
        currentPageIndex = 0;

        PlayerMovement.Instance?.SetMovementLocked(true);
        ShowOwnerPage(currentPageIndex);
    }

    /// <summary>
    /// Force-closes the conversation immediately (e.g. owner's patience ran out).
    /// Only acts if this owner's box is the one currently active.
    /// </summary>
    public void CancelConversationFor(DialogueBox ownerBox)
    {
        if (activeOwnerBox != ownerBox) return;

        activeOwnerBox?.Hide();
        PlayerReactionBox.Instance?.Hide();
        nextButton.gameObject.SetActive(false);
        waitingForNext = false;
        activeOwnerBox = null;
        pages = null;

        PlayerMovement.Instance?.SetMovementLocked(false);
    }

    private void ShowOwnerPage(int index)
    {
        if (index < 0 || index >= pages.Count)
        {
            EndConversation();
            return;
        }

        waitingForNext = false;
        nextButton.gameObject.SetActive(false);
        activeOwnerBox.ShowPage(pages[index], OnOwnerLineFinishedTyping);
    }

    private void OnOwnerLineFinishedTyping()
    {
        waitingForNext = true;
        nextButton.gameObject.SetActive(true);
    }

    private void OnNextClicked()
    {
        // Clicking Next while something is mid-type just finishes it instantly.
        if (activeOwnerBox != null && activeOwnerBox.IsTyping)
        {
            activeOwnerBox.CompleteTyping();
            return;
        }
        if (PlayerReactionBox.Instance != null && PlayerReactionBox.Instance.IsTyping)
        {
            PlayerReactionBox.Instance.CompleteTyping();
            return;
        }

        if (!waitingForNext) return;

        AdvanceConversation();
    }

    private void OnSkipClicked()
    {
        if (activeOwnerBox != null && activeOwnerBox.IsTyping)
        {
            activeOwnerBox.CompleteTyping();
        }
        else if (PlayerReactionBox.Instance != null && PlayerReactionBox.Instance.IsTyping)
        {
            PlayerReactionBox.Instance.CompleteTyping();
        }
    }

    private void AdvanceConversation()
    {
        waitingForNext = false;
        nextButton.gameObject.SetActive(false);

        currentPageIndex++;
        bool isLastPage = currentPageIndex >= pages.Count;

        activeOwnerBox.Hide(); // owner's line steps aside while the player responds

        string[] lineBank = isLastPage ? farewellLines : advanceLines;
        string line = (lineBank != null && lineBank.Length > 0)
            ? lineBank[UnityEngine.Random.Range(0, lineBank.Length)]
            : null;

        if (string.IsNullOrEmpty(line))
        {
            OnPlayerLineFinished(isLastPage);
            return;
        }

        PlayerReactionBox.Instance.ShowLine(line, 0f, () => OnPlayerLineFinished(isLastPage));
    }

    private void OnPlayerLineFinished(bool isLastPage)
    {
        if (isLastPage)
        {
            EndConversation();
        }
        else
        {
            PlayerReactionBox.Instance?.Hide();
            ShowOwnerPage(currentPageIndex);
        }
    }

    private void EndConversation()
    {
        activeOwnerBox?.Hide();
        activeOwnerBox = null;
        pages = null;
        nextButton.gameObject.SetActive(false);
        waitingForNext = false;

        PlayerMovement.Instance?.SetMovementLocked(false);
        PlayerReactionBox.Instance?.HideAfterDelay(1.5f); // let the farewell line linger briefly
    }
}