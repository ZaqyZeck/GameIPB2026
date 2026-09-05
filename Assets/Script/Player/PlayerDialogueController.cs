using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDialogueController : MonoBehaviour
{
    public static PlayerDialogueController Instance;

    [Header("References")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

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
        skipButton.onClick.AddListener(OnSkipClicked);
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
    }

    public void StartConversation(DialogueBox ownerBox, List<DialoguePage> newPages, string[] newAdvanceLines, string[] newFarewellLines)
    {
        activeOwnerBox = ownerBox;
        pages = newPages;
        advanceLines = newAdvanceLines;
        farewellLines = newFarewellLines;
        currentPageIndex = 0;

        PlayerMovement.Instance?.SetMovementLocked(true);

        // Both boxes come up together and stay up for the whole conversation.
        PlayerReactionBox.Instance?.ShowIdle();
        ShowOwnerPage(currentPageIndex);
    }

    public void CancelConversationFor(DialogueBox ownerBox)
    {
        if (activeOwnerBox != ownerBox) return;

        activeOwnerBox?.Hide();
        PlayerReactionBox.Instance?.Hide();
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
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
        skipButton.gameObject.SetActive(true);
        activeOwnerBox.ShowPage(pages[index], OnOwnerLineFinishedTyping);
    }

    private void OnOwnerLineFinishedTyping()
    {
        waitingForNext = true;
        nextButton.gameObject.SetActive(true);
        skipButton.gameObject.SetActive(false);
    }

    private void OnNextClicked()
    {
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

        // No hiding here anymore - the owner box just keeps showing its last
        // line while the player's box updates with a new one.
        string[] lineBank = isLastPage ? farewellLines : advanceLines;
        string line = (lineBank != null && lineBank.Length > 0)
            ? lineBank[UnityEngine.Random.Range(0, lineBank.Length)]
            : null;

        skipButton.gameObject.SetActive(!string.IsNullOrEmpty(line));

        if (string.IsNullOrEmpty(line))
        {
            OnPlayerLineFinished(isLastPage);
            return;
        }

        PlayerReactionBox.Instance.ShowLine(line, () => OnPlayerLineFinished(isLastPage));
    }

    private void OnPlayerLineFinished(bool isLastPage)
    {
        skipButton.gameObject.SetActive(false);

        if (isLastPage)
        {
            EndConversation();
        }
        else
        {
            // Owner box updates to the next page while player's box just keeps
            // showing its last reaction line underneath - nothing gets hidden.
            ShowOwnerPage(currentPageIndex);
        }
    }

    private void EndConversation()
    {
        activeOwnerBox?.Hide();
        activeOwnerBox = null;
        pages = null;
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        waitingForNext = false;

        PlayerMovement.Instance?.SetMovementLocked(false);
        PlayerReactionBox.Instance?.HideAfterDelay(1.5f);
    }
}