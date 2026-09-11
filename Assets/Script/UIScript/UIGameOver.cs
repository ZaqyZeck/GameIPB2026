using UnityEngine;
using UnityEngine.UI;
using Ohm.UISystem;
using TMPro;

public class UIGameOver : UIBase
{
    [Header("References")]
    [SerializeField] private Button saveButton;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI scoreText;
    bool isSubmitting = false;

    void Awake()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveButton);
    }

    private void OnEnable()
    {
        // Panel ini baru aktif saat game over, jadi event reputasi selama
        // gameplay terlewat. Ambil nilai terakhir yang disimpan UIGameplay.
        ShowScore(UIGameplay.Reputation);

        // Tetap dengarkan kalau reputasi masih berubah setelah panel muncul
        GameEventBus.OnReputationChange += HandleReputationChange;

        if (saveButton != null)
            saveButton.interactable = true;
    }

    private void OnDisable()
    {
        GameEventBus.OnReputationChange -= HandleReputationChange;
        isSubmitting = false;
    }

    private void HandleReputationChange(int before, int after)
    {
        ShowScore(after);
    }

    private void ShowScore(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    private void SaveButton()
    {
        if (isSubmitting) return;
        string playerName = nameInputField != null ? nameInputField.text.Trim() : string.Empty;
        if (string.IsNullOrEmpty(playerName))
            playerName = "Player";

        saveButton.interactable = false; // cegah submit dua kali

        isSubmitting = true;
        GameEventBus.OnSubmitScore?.Invoke(playerName);
        //UIGameplay.ResetReputation();

        //GameManager.Instance.LoadMainMenu();
    }
}