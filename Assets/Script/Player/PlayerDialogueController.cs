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
    [SerializeField] private Button prevButton;
    [SerializeField] private Button skipButton;

    private DialogueBox activeOwnerBox;
    private List<DialoguePage> pages;
    private int currentPageIndex;
    private string[] advanceLines;
    private string[] farewellLines;
    private Owner currentOwner;

    private bool waitingForNext;

    private void Awake()
    {
        Instance = this;
        nextButton.onClick.AddListener(OnNextClicked);
        if (prevButton != null)
        {
            prevButton.onClick.AddListener(OnPrevClicked);
            prevButton.gameObject.SetActive(false);
        }
        skipButton.onClick.AddListener(OnSkipClicked);
        nextButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnFastForward += HandleFastForward;
            GameInputManager.Instance.OnDialoguePrev += HandlePrevInput;
            GameInputManager.Instance.OnDialogueNext += HandleNextInput;
        }
    }

    private void Start()
    {
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnFastForward -= HandleFastForward;
            GameInputManager.Instance.OnFastForward += HandleFastForward;
            GameInputManager.Instance.OnDialoguePrev -= HandlePrevInput;
            GameInputManager.Instance.OnDialoguePrev += HandlePrevInput;
            GameInputManager.Instance.OnDialogueNext -= HandleNextInput;
            GameInputManager.Instance.OnDialogueNext += HandleNextInput;
        }
    }

    private void OnDisable()
    {
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.OnFastForward -= HandleFastForward;
            GameInputManager.Instance.OnDialoguePrev -= HandlePrevInput;
            GameInputManager.Instance.OnDialogueNext -= HandleNextInput;
        }
    }

    private void HandleFastForward()
    {
        if (LevelManager.Instance != null && !LevelManager.Instance.IsPlaying) return;

        if (activeOwnerBox != null && activeOwnerBox.IsTyping)
        {
            activeOwnerBox.CompleteTyping();
        }
        else if (PlayerReactionBox.Instance != null && PlayerReactionBox.Instance.IsTyping)
        {
            PlayerReactionBox.Instance.CompleteTyping();
        }
    }

    private void HandleNextInput()
    {
        if (LevelManager.Instance != null && !LevelManager.Instance.IsPlaying) return;

        if (waitingForNext)
        {
            OnNextClicked();
        }
    }

    private void HandlePrevInput()
    {
        if (LevelManager.Instance != null && !LevelManager.Instance.IsPlaying) return;

        int minPrevIndex = (pages != null && pages.Count > 1) ? 1 : 0;
        if (waitingForNext && currentPageIndex > minPrevIndex)
        {
            OnPrevClicked();
        }
    }

    public void StartConversation(DialogueBox ownerBox, List<DialoguePage> newPages, string[] newAdvanceLines, string[] newFarewellLines, Owner owner)
    {
        activeOwnerBox = ownerBox;
        pages = newPages;
        advanceLines = newAdvanceLines;
        farewellLines = newFarewellLines;
        currentOwner = owner;

        // Resume dialogue if this owner has already been spoken to, starting from where it left off (skipping intro greeting)
        if (currentOwner != null && currentOwner.hasTalkedBefore && newPages != null && newPages.Count > 0)
        {
            int minIndex = newPages.Count > 1 ? 1 : 0;
            currentPageIndex = Mathf.Clamp(currentOwner.lastDialogueIndex, minIndex, newPages.Count - 1);
        }
        else
        {
            currentPageIndex = 0;
            if (currentOwner != null)
            {
                currentOwner.hasTalkedBefore = true;
                currentOwner.lastDialogueIndex = 0;
            }
        }

        PlayerMovement.Instance?.SetMovementLocked(true);

        skipButton.gameObject.SetActive(true);

        PlayerReactionBox.Instance?.ShowIdle();
        ShowOwnerPage(currentPageIndex);
    }

    public void CancelConversationFor(DialogueBox ownerBox)
    {
        if (activeOwnerBox != ownerBox) return;

        if (currentOwner != null && pages != null && pages.Count > 0)
        {
            int minIndex = pages.Count > 1 ? 1 : 0;
            currentOwner.lastDialogueIndex = Mathf.Clamp(currentPageIndex, minIndex, pages.Count - 1);
        }

        activeOwnerBox?.Hide();
        PlayerReactionBox.Instance?.Hide();
        nextButton.gameObject.SetActive(false);
        if (prevButton != null) prevButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        waitingForNext = false;
        activeOwnerBox = null;
        pages = null;
        currentOwner = null;

        PlayerMovement.Instance?.SetMovementLocked(false);
    }

    private void ShowOwnerPage(int index, bool instant = false)
    {
        if (index < 0 || index >= pages.Count)
        {
            EndConversation();
            return;
        }

        waitingForNext = false;
        nextButton.gameObject.SetActive(false);
        if (prevButton != null) prevButton.gameObject.SetActive(false);

        activeOwnerBox.ShowPage(pages[index], OnOwnerLineFinishedTyping);

        if (instant)
        {
            activeOwnerBox.CompleteTyping();
        }
    }

    private void OnOwnerLineFinishedTyping()
    {
        waitingForNext = true;
        nextButton.gameObject.SetActive(true);
        if (prevButton != null)
        {
            int minPrevIndex = (pages != null && pages.Count > 1) ? 1 : 0;
            prevButton.gameObject.SetActive(currentPageIndex > minPrevIndex);
        }
    }

    private void OnNextClicked()
    {
        if (!waitingForNext) return;
        AdvanceConversation();
    }

    private void OnPrevClicked()
    {
        int minPrevIndex = (pages != null && pages.Count > 1) ? 1 : 0;
        if (!waitingForNext || currentPageIndex <= minPrevIndex) return;

        currentPageIndex--;
        if (currentOwner != null)
        {
            currentOwner.lastDialogueIndex = currentPageIndex;
        }

        ShowOwnerPage(currentPageIndex, instant: true);
    }

    private void OnSkipClicked()
    {
        EndConversation();
    }

    private void AdvanceConversation()
    {
        waitingForNext = false;
        nextButton.gameObject.SetActive(false);
        if (prevButton != null) prevButton.gameObject.SetActive(false);

        currentPageIndex++;
        if (currentOwner != null) 
        {
            currentOwner.lastDialogueIndex = currentPageIndex;
            if (currentOwner.dialogCounter < currentPageIndex) currentOwner.dialogCounter = currentPageIndex;
        }

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
        if (currentOwner != null && pages != null && pages.Count > 0)
        {
            int minIndex = pages.Count > 1 ? 1 : 0;
            currentOwner.lastDialogueIndex = Mathf.Clamp(currentPageIndex, minIndex, pages.Count - 1);
        }

        activeOwnerBox?.Hide();
        activeOwnerBox = null;
        pages = null;
        nextButton.gameObject.SetActive(false);
        if (prevButton != null) prevButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        waitingForNext = false;

        PlayerMovement.Instance?.SetMovementLocked(false);
        PlayerReactionBox.Instance?.HideAfterDelay(1.5f);
    }
}