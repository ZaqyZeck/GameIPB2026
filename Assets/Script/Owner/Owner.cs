using DG.Tweening;
using TMPro;
using UnityEngine;

public class Owner : Interactables
{
    [SerializeField] string ownerName;
    //[SerializeField] int petId;
    [SerializeField] float patienceAmount = 60f;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Collider2D interactCollider;
    [SerializeField] TextMeshPro textPetId;

    [Header("Dialogue")]
    [SerializeField] DialogueBox dialogueBox; // the DialogueCanvas child under this Owner
    [SerializeField] Sprite catIcon;          // icon shown on the "Do you see my cat?" page

    Pet currentPet;
    //IHoldable currentPet;

    OwnerData currentOwnerData;
    
    public bool isInLine;

    float patienceTimer;

    private void Awake()
    {
        patienceTimer = patienceAmount;
    }

    private void Update()
    {
        if(patienceTimer >= 0 && isInLine)
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

    void OpenDialogue()
    {
        if (currentPet == null || dialogueBox == null) return;

        string habitText = (currentPet.CurrentHabit as IDialogueDescribable)?.GetDialogueText()
                            ?? "Hmm, not sure what it likes to do.";
        string actionText = (currentPet.CurrentAction as IDialogueDescribable)?.GetDialogueText()
                            ?? "Hmm, not sure what it does.";

        System.Collections.Generic.List<DialoguePage> pages = new System.Collections.Generic.List<DialoguePage>
        {
            new DialoguePage { text = "Do you see my cat?", icon = catIcon },
            new DialoguePage { text = habitText, icon = null },
            new DialoguePage { text = actionText, icon = null },
        };

        dialogueBox.Show(pages);
    }

    public bool GetPet(IHoldable heldPet)
    {
        if (currentPet == null || !ReferenceEquals(heldPet, currentPet))
        {
            Debug.Log("Pet salah, ini bukan pet yang diminta " + ownerName);
            DespawnWithoutPet() ;
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

        //Debug.Log(ownerName + " spawn");
        SpawnAnimation();
    }
    public void DespawnWithoutPet()
    {
        if (!isInLine) return;

        //Despawn pet
        //PetManager.Instance.DespawnPet(currentPet);

        dialogueBox?.Hide();

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

        //Despawn pet
        PetManager.Instance.DespawnPet(currentPet);

        dialogueBox?.Hide();

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

    void SpawnAnimation()
    {
        spriteRenderer.DOFade(1f, 1f);
    }
    void DespawnAnimation()
    {
        spriteRenderer.DOFade(0f, 1f);
    }
    public OwnerData GetOwnerData()
    {
        return currentOwnerData;
    }

}