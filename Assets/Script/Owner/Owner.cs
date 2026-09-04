using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Owner : Interactables
{
    [SerializeField] private string ownerName;
    [SerializeField] private float patienceAmount = 60f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D interactCollider;
    [SerializeField] private TextMeshPro textPetId;

    [Header("Dialogue")]
    [SerializeField] private DialogueBox dialogueBox; // this owner's own textbox above them
    [SerializeField] private Sprite catIcon;

    [Header("Player Reaction Lines")]
    [SerializeField]
    private string[] advanceLines =
    {
        "Could you tell me more?",
        "Go on...",
        "Hmm, tell me more."
    };
    [SerializeField]
    private string[] farewellLines =
    {
        "Okay, I'll be right back.",
        "Got it, thank you!",
        "Alright, I'll go look."
    };

    private Pet currentPet;
    private OwnerData currentOwnerData;

    public bool isInLine;
    private float patienceTimer;

    private void Awake()
    {
        patienceTimer = patienceAmount;
    }

    private void Update()
    {
        if (patienceTimer >= 0 && isInLine)
        {
            patienceTimer -= Time.deltaTime;
        }
        else
        {
            patienceTimer = patienceAmount;
            DespawnWithoutPet();
        }
    }

    public override void OnInteract(PlayerInteract player)
    {
        if (player.isHoldingObject)
        {
            player.GivePet(); // already holding a pet -> treat click as "hand it over"
        }
        else
        {
            OpenDialogue();
        }
    }

    private void OpenDialogue()
    {
        if (currentPet == null || currentPet.petData == null) return;
        if (dialogueBox == null || PlayerDialogueController.Instance == null) return;

        IHabitBehavior habit = PetBehaviorFactory.GetHabitBehavior(currentPet.petData.hiddenHabit);
        IActionBehavior action = PetBehaviorFactory.GetActionBehavior(currentPet.petData.hiddenAction);

        string habitText = (habit as IDialogueDescribable)?.GetDialogueText()
                            ?? "Hmm, not sure what it likes to do.";
        string actionText = (action as IDialogueDescribable)?.GetDialogueText()
                            ?? "Hmm, not sure what it does.";

        List<DialoguePage> pages = new List<DialoguePage>
        {
            new DialoguePage { text = "Do you see my cat?", icon = catIcon },
            new DialoguePage { text = habitText, icon = null },
            new DialoguePage { text = actionText, icon = null },
        };

        PlayerDialogueController.Instance.StartConversation(dialogueBox, pages, advanceLines, farewellLines);
    }

    public bool GetPet(IHoldable heldPet)
    {
        if (currentPet == null || !ReferenceEquals(heldPet, currentPet))
        {
            Debug.Log("Pet salah, ini bukan pet yang diminta " + ownerName);
            DespawnWithoutPet();
            return false;
        }

        Debug.Log("berhasil dapat pet");
        DespawnWithPet();
        return true;
    }

    public void Spawn(Pet wantedPet, OwnerData newOwnerData)
    {
        if (isInLine) return;

        currentPet = wantedPet;
        currentOwnerData = newOwnerData;
        ownerName = currentOwnerData.ownerName;
        textPetId.text = currentPet.petId.ToString();
        interactCollider.enabled = true;
        isInLine = true;

        SpawnAnimation();
    }

    public void DespawnWithoutPet()
    {
        if (!isInLine) return;

        PlayerDialogueController.Instance?.CancelConversationFor(dialogueBox);
        textPetId.text = null;
        currentPet = null;
        currentOwnerData = null;
        ownerName = null;
        interactCollider.enabled = false;
        isInLine = false;

        ReputationManager.Instance.Penalize(100);
        DespawnAnimation();
        OwnerManager.Instance.CheckLine();
    }

    public void DespawnWithPet()
    {
        if (!isInLine) return;

        PetManager.Instance.DespawnPet(currentPet);
        PlayerDialogueController.Instance?.CancelConversationFor(dialogueBox);
        textPetId.text = null;
        currentPet.isOwnerArrived = false;
        currentPet = null;
        currentOwnerData = null;
        ownerName = null;
        interactCollider.enabled = false;
        isInLine = false;

        ReputationManager.Instance.Reward(100);
        DespawnAnimation();
        OwnerManager.Instance.CheckLine();
    }

    private void SpawnAnimation()
    {
        spriteRenderer.DOFade(1f, 1f);
    }

    private void DespawnAnimation()
    {
        spriteRenderer.DOFade(0f, 1f);
    }

    public OwnerData GetOwnerData()
    {
        return currentOwnerData;
    }
}