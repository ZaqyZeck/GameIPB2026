using Ohm.UISystem;
using UnityEngine;
using UnityEngine.UI;

public class UIDifficultySelection : UIBase
{
    [SerializeField] private Button backButton;

    [Header("Difficulty Button")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button adaptButton;

    [Header("Difficulty Profiles")]
    [SerializeField] private DifficultyProfileSO easyProfile;
    [SerializeField] private DifficultyProfileSO normalProfile;
    [SerializeField] private DifficultyProfileSO adaptProfile;

    void Awake()
    {
        if (backButton != null) backButton.onClick.AddListener(BackButtonPressed);
        if (easyButton != null) easyButton.onClick.AddListener(EasyButtonPressed);
        if (normalButton != null) normalButton.onClick.AddListener(NormalButtonPressed);
        if (adaptButton != null) adaptButton.onClick.AddListener(AdaptButtonPressed);
    }

    public void BackButtonPressed()
    {
        UIManager.Instance.OnEscape();
    }

    public void EasyButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            if (easyProfile != null) GameManager.Instance.SetDifficulty(easyProfile);
            else GameManager.Instance.SetDifficulty(DifficultyTier.Easy);

            GameManager.Instance.LoadScene(SceneType.Gameplay);
        }
    }

    public void NormalButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            if (normalProfile != null) GameManager.Instance.SetDifficulty(normalProfile);
            else GameManager.Instance.SetDifficulty(DifficultyTier.Medium);

            GameManager.Instance.LoadScene(SceneType.Gameplay);
        }
    }

    public void AdaptButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            if (adaptProfile != null) GameManager.Instance.SetDifficulty(adaptProfile);
            else GameManager.Instance.SetDifficulty(DifficultyTier.Adaptive);

            GameManager.Instance.LoadScene(SceneType.Gameplay);
        }
    }
}
