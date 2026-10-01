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

    void Awake()
    {
        //if (resumeButton != null)
        //    resumeButton.onClick.AddListener(ResumeButton);
        //if (homeButton != null)
        //    homeButton.onClick.AddListener(BackToMenu);
        if (backButton != null) backButton.onClick.AddListener(BackButtonPressed);
        if (easyButton != null) easyButton.onClick.AddListener(EasyButtonPressed);
        if (normalButton != null) normalButton.onClick.AddListener(NormalButtonPressed);
        if (adaptButton != null) adaptButton.onClick.AddListener(AdaptButtonPressed);
    }

    public void BackButtonPressed()
    {
        UIManager.Instance.OnEscape();
        //GameEventBus.OnResume?.Invoke();
    }
    public void EasyButtonPressed()
    {
        GameManager.Instance.LoadScene(SceneType.Gameplay);
        Debug.Log("Start Game Clicked");
        //UIGameplay.ResetReputation();

        //GameManager.Instance.LoadMainMenu();
    }
    public void NormalButtonPressed()
    {
        GameManager.Instance.LoadScene(SceneType.Gameplay);
        Debug.Log("Start Game Clicked");
        //UIGameplay.ResetReputation();

        //GameManager.Instance.LoadMainMenu();
    }
    public void AdaptButtonPressed()
    {
        GameManager.Instance.LoadScene(SceneType.Gameplay);
        Debug.Log("Start Game Clicked");
        //UIGameplay.ResetReputation();

        //GameManager.Instance.LoadMainMenu();
    }
}
