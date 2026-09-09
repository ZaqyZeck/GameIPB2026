using UnityEngine;
using UnityEngine.UI;
using Ohm.UISystem;

public class UIPauseMenu : UIBase
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button homeButton;

    void Awake()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeButton);
        if (homeButton != null)
            homeButton.onClick.AddListener(BackToMenu);
    }

    public void ResumeButton()
    {
        UIManager.Instance.OnEscape();
    }
    public void BackToMenu()
    {
        // UIManager.Instance.ShowUI(UIType.UISettings);
    }
}
