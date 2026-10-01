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
    private float totalPatience;
    private float maxFillWidth;

    public int dialogCounter = 0;
    public int lastDialogueIndex = 0;
    public bool hasTalkedBefore = false;
    private float spawnTimestamp;

    private void Awake()
    {
        totalPatience = patienceAmount + 30f;
        patienceTimer = totalPatience;
        
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
        if (!LevelManager.Instance.IsPlaying) return;
        if (patienceTimer >= 0 && isInLine)
        {
            patienceTimer -= Time.deltaTime;
            UpdatePatienceUI();
        }
        else if (isInLine)
        {
            patienceTimer = totalPatience;
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
            float fillPercentage = totalPatience > 0 ? (patienceTimer / totalPatience) : 0f;
            patienceFillRect.sizeDelta = new Vector2(maxFillWidth * fillPercentage, patienceFillRect.sizeDelta.y);
        }
    }

     public override void OnInteract(PlayerInteract player)
    {
        if (player.isHoldingObject && player.CurrentHeldHoldable is Pet) player.GivePet();
        else OpenDialogue();
    }

        private void OpenDialogue()
    {
        if (currentPet == null || currentPet.petData == null || dialogueBox == null || PlayerDialogueController.Instance == null) return;

        List<DialoguePage> pages = PetClueGenerator.GenerateDialoguePages(currentPet, this);

        PlayerDialogueController.Instance.StartConversation(dialogueBox, pages, advanceLines, farewellLines, this);
    }

    public bool GetPet(IHoldable heldPet)
    {
        if (currentPet == null || !ReferenceEquals(heldPet, currentPet))
        {
            DespawnWithoutPet();
            return false;
        }
        float solveDuration = Time.time - spawnTimestamp;
        OwnerManager.RecordSolve(solveDuration);
        DespawnWithPet();
        return true;
    }

    public void Spawn(Pet wantedPet, OwnerData newOwnerData)
    {
        if (isInLine) return;

        dialogCounter = 0;
        lastDialogueIndex = 0;
        hasTalkedBefore = false;
        spawnTimestamp = Time.time;
        currentPet = wantedPet;
        currentOwnerData = newOwnerData;
        ownerName = currentOwnerData.ownerName;
        currentPetId = currentPet.petId;

        if (textPetId != null) textPetId.text = currentPet.petId.ToString();

        ApplyOwnerSprite();

        float extraPatience = 30f;
        if (PetManager.Instance != null && PetManager.Instance.DifficultyProfile != null)
        {
            extraPatience = PetManager.Instance.DifficultyProfile.extraPatienceTime;
        }

        totalPatience = patienceAmount + extraPatience;
        patienceTimer = totalPatience;
        if (patienceFillRect != null) patienceFillRect.sizeDelta = new Vector2(maxFillWidth, patienceFillRect.sizeDelta.y);

        interactCollider.enabled = true;
        isInLine = true;

        SpawnAnimation();
    }

    private void ApplyOwnerSprite()
    {
        if (spriteRenderer == null || currentPet == null || currentPet.petData == null) return;

        Sprite ownerSprite = currentPet.petData.ownerSprite;

        if (ownerSprite != null) spriteRenderer.sprite = ownerSprite;
    }

    public void DespawnWithoutPet()
    {
        if (!isInLine) return;
        if (currentPet != null)
        {
            currentPet.ResetOwnerArrived();
        }
        Vector3 spawnWorldPos = transform.position + Vector3.up * 1.6f;
        ResetOwnerState();
        DespawnAnimation();
        OwnerManager.Instance.CheckLine();
        int penalty = (patienceTimer > 0) ? 15 : 17;
        dialogCounter = 0;

        bool isFatal = HealthManager.Instance != null && HealthManager.Instance.GetHealth() <= 1;
        if (isFatal)
        {
            if (UIGameplay.Instance != null)
            {
                UIGameplay.Instance.CompleteAllFlyingScores();
            }
            ReputationManager.Instance.Penalize(penalty);
            if (UIGameplay.Instance != null)
            {
                UIGameplay.Instance.RefreshDisplay();
            }
        }
        else
        {
            ReputationManager.Instance.Penalize(penalty, spawnWorldPos);
        }

        HealthManager.Instance.TakeDamage();
    }

    public void DespawnWithPet()
    {
        if (!isInLine) return;
        Vector3 spawnWorldPos = transform.position + Vector3.up * 1.6f;
        PetManager.Instance.DespawnPet(currentPet);
        ResetOwnerState();
        DespawnAnimation();
        OwnerManager.Instance.CheckLine();
        int score = 0;

        float ratio = totalPatience > 0 ? (patienceTimer / totalPatience) : 0f;
        if (ratio > 0.6f) score = 15;
        else if (ratio > 0.3f) score = 10;
        else score = 5;

        if (dialogCounter <= 0) score += 15;
        else if (dialogCounter == 1) score += 10;
        else if (dialogCounter == 2) score += 5;

        dialogCounter = 0;
        ReputationManager.Instance.Reward(score, spawnWorldPos);
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
        dialogCounter = 0;
        lastDialogueIndex = 0;
        hasTalkedBefore = false;
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