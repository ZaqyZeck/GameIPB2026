using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Added for the New Input System

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

    private void Update()
    {
        bool fastForwardInput = false;

        // Check for any keyboard key, left mouse click, or the bottom gamepad button (A/Cross)
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            fastForwardInput = true;
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            fastForwardInput = true;
        }
        else if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            fastForwardInput = true;
        }

        // Fast-forward typing
        if (fastForwardInput)
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
    }

    public void StartConversation(DialogueBox ownerBox, List<DialoguePage> newPages, string[] newAdvanceLines, string[] newFarewellLines)
    {
        activeOwnerBox = ownerBox;
        pages = newPages;
        advanceLines = newAdvanceLines;
        farewellLines = newFarewellLines;
        currentPageIndex = 0;

        PlayerMovement.Instance?.SetMovementLocked(true);

        skipButton.gameObject.SetActive(true);

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
        activeOwnerBox.ShowPage(pages[index], OnOwnerLineFinishedTyping);
    }

    private void OnOwnerLineFinishedTyping()
    {
        waitingForNext = true;
        nextButton.gameObject.SetActive(true);
    }

    private void OnNextClicked()
    {
        if (!waitingForNext) return;
        AdvanceConversation();
    }

    private void OnSkipClicked()
    {
        EndConversation();
    }

    private void AdvanceConversation()
    {
        waitingForNext = false;
        nextButton.gameObject.SetActive(false);

        currentPageIndex++;
        bool isLastPage = currentPageIndex >= pages.Count;

        string[] lineBank = isLastPage ? farewellLines : advanceLines;
        string line = (lineBank != null && lineBank.Length > 0)
            ? lineBank[UnityEngine.Random.Range(0, lineBank.Length)]
            : null;

        if (string.IsNullOrEmpty(line))
        {
            OnPlayerLineFinished(isLastPage);
            return;
        }

        PlayerReactionBox.Instance.ShowLine(line, () => OnPlayerLineFinished(isLastPage));
    }

    private void OnPlayerLineFinished(bool isLastPage)
    {
        if (isLastPage)
        {
            EndConversation();
        }
        else
        {
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