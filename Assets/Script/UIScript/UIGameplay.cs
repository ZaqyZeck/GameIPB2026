using UnityEngine;
using UnityEngine.UI;
using Ohm.UISystem;
using TMPro;


public class UIGameplay : UIBase
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private TextMeshProUGUI scoreText;

    void Awake()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OpenPauseMenu);
    }

    public void OpenPauseMenu()
    {
        UIManager.Instance.OnEscape();
    }
    public void RotateLeft()
    {
        if (OhmUISystemDemoManager.Instance == null) return;
        OhmUISystemDemoManager.Instance.RotateLeft();
    }

    public void RotateRight()
    {
        if (OhmUISystemDemoManager.Instance == null) return;
        OhmUISystemDemoManager.Instance.RotateRight();
    }
}
