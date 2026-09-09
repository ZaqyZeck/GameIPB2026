using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Owner : Interactables
{
    [SerializeField] private string ownerName;
    [SerializeField] private float patienceAmount = 60f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D interactCollider;
    [SerializeField] private TextMeshPro textPetId;
    [SerializeField] private Material ownerMaterial;

    [Header("Patience UI")]
    [Tooltip("Assign the 'Fill Mask' here so it shrinks over time.")]
    [SerializeField] private RectTransform patienceFillRect;
    [Tooltip("Assign the root GameObject of the entire UI bar here to turn it on/off.")]
    [SerializeField] private GameObject patienceBarObject; 

    [Header("Dialogue")]
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private Sprite catIcon;

    [Header("Player Reaction Lines")]
    [SerializeField]
    private string[] advanceLines = { "Could you tell me more?", "Go on...", "Hmm, tell me more." };
    [SerializeField]
    private string[] farewellLines = { "Okay, I'll be right back.", "Got it, thank you!", "Alright, I'll go look." };

    private Pet currentPet;
    [SerializeField] int currentPetId;
    private OwnerData currentOwnerData;

    public bool isInLine;
    private float patienceTimer;
    private float maxFillWidth;

    private void Awake()
    {
        patienceTimer = patienceAmount;
        
        if (patienceFillRect != null)
        {
            maxFillWidth = patienceFillRect.sizeDelta.x;
        }

        // Hide the bar completely on awake
        if (patienceBarObject != null)
        {
            patienceBarObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (patienceTimer >= 0 && isInLine)
        {
            patienceTimer -= Time.deltaTime;
            UpdatePatienceUI();
        }
        else if (isInLine)
        {
            patienceTimer = patienceAmount;
            DespawnWithoutPet();
        }
    }

    private void Start()
    {
        if (ownerMaterial != null) ownerMaterial.SetFloat("_outlineOn", 0f);
    }

    private void UpdatePatienceUI()
    {
        if (patienceFillRect != null)
        {
            float fillPercentage = patienceTimer / patienceAmount;
            patienceFillRect.sizeDelta = new Vector2(maxFillWidth * fillPercentage, patienceFillRect.sizeDelta.y);
        }
    }

    public override void OnInteract(PlayerInteract player)
    {
        if (player.isHoldingObject) player.GivePet();
        else OpenDialogue();
    }

    private void OpenDialogue()
    {
        if (currentPet == null || currentPet.petData == null || dialogueBox == null || PlayerDialogueController.Instance == null) return;

        IHabitBehavior habit = PetBehaviorFactory.GetHabitBehavior(currentPet.petData.hiddenHabit);
        IActionBehavior action = PetBehaviorFactory.GetActionBehavior(currentPet.petData.hiddenAction);

        string habitText = (habit as IDialogueDescribable)?.GetDialogueText() ?? "Hmm, not sure what it likes to do.";
        string actionText = (action as IDialogueDescribable)?.GetDialogueText() ?? "Hmm, not sure what it does.";

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
            DespawnWithoutPet();
            return false;
        }
        DespawnWithPet();
        return true;
    }

    public void Spawn(Pet wantedPet, OwnerData newOwnerData)
    {
        if (isInLine) return;

        currentPet = wantedPet;
        currentOwnerData = newOwnerData;
        ownerName = currentOwnerData.ownerName;
        currentPetId = currentPet.petId;

        if (textPetId != null) textPetId.text = currentPet.petId.ToString();

        patienceTimer = patienceAmount;
        if (patienceFillRect != null) patienceFillRect.sizeDelta = new Vector2(maxFillWidth, patienceFillRect.sizeDelta.y);

        interactCollider.enabled = true;
        isInLine = true;

        SpawnAnimation();
    }

    public void DespawnWithoutPet()
    {
        if (!isInLine) return;
        ReputationManager.Instance.Penalize(100);
        ResetOwnerState();
        DespawnAnimation();
        OwnerManager.Instance.CheckLine();
    }

    public void DespawnWithPet()
    {
        if (!isInLine) return;
        PetManager.Instance.DespawnPet(currentPet);
        //currentPet.isOwnerArrived = false;
        ReputationManager.Instance.Reward(100);
        ResetOwnerState();
        DespawnAnimation();
        OwnerManager.Instance.CheckLine();
    }

    private void ResetOwnerState()
    {
        PlayerDialogueController.Instance?.CancelConversationFor(dialogueBox);
        if (textPetId != null) textPetId.text = null;
        currentPet = null;
        currentOwnerData = null;
        ownerName = null;
        interactCollider.enabled = false;
        isInLine = false;
    }

    private void SpawnAnimation()
    {
        spriteRenderer.DOFade(1f, 1f);
        if (patienceBarObject != null) patienceBarObject.SetActive(true); // Turn on instantly
    }

    private void DespawnAnimation()
    {
        spriteRenderer.DOFade(0f, 1f);
        if (patienceBarObject != null) patienceBarObject.SetActive(false); // Turn off instantly
    }

    public OwnerData GetOwnerData()
    {
        return currentOwnerData;
    }


    public override void OnSelectedHover()
    {
        TurnOnOutline(true);
    }

    public override void OnDeselectedHover()
    {
        TurnOnOutline(false);
    }

    void TurnOnOutline(bool isOn)
    {
        if (ownerMaterial == null) return;
        if (isOn)
        {
            ownerMaterial.SetFloat("_outlineOn", 1.0f);
        }
        else
        {
            //Debug.Log("dawdawwdwawda");
            ownerMaterial.SetFloat("_outlineOn", 0f);
        }
    }
}