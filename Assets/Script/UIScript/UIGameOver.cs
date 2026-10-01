using UnityEngine;
using UnityEngine.UI;
using Ohm.UISystem;
using TMPro;

using DG.Tweening;

public class UIGameOver : UIBase
{
    [Header("References")]
    [SerializeField] private Button saveButton;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Score Animation")]
    [SerializeField] private float scoreCountDuration = 1.0f;
    [SerializeField] private Ease scoreCountEase = Ease.OutQuad;

    private Tween scoreTween;
    bool isSubmitting = false;

    void Awake()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveButton);
    }

    private void OnEnable()
    {
        int finalScore = (ReputationManager.Instance != null) ? ReputationManager.Instance.GetScore() : UIGameplay.Reputation;
        AnimateScore(finalScore);

        GameEventBus.OnReputationChange += HandleReputationChange;

        if (saveButton != null)
            saveButton.interactable = true;
    }

    private void OnDisable()
    {
        scoreTween?.Kill();
        GameEventBus.OnReputationChange -= HandleReputationChange;
        isSubmitting = false;
    }

    private void HandleReputationChange(int before, int after)
    {
        AnimateScore(after);
    }

    private void AnimateScore(int targetScore)
    {
        scoreTween?.Kill();

        if (scoreText == null) return;

        scoreText.text = "0";

        if (targetScore == 0) return;

        int current = 0;
        float duration = Mathf.Clamp(scoreCountDuration, 0.4f, 2.0f);

        scoreTween = DOTween.To(() => current, val =>
        {
            current = val;
            if (scoreText != null)
                scoreText.text = current.ToString();
        }, targetScore, duration)
        .SetEase(scoreCountEase)
        .SetUpdate(true);
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