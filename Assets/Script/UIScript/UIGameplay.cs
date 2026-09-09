using UnityEngine;
using UnityEngine.UI;
using Ohm.UISystem;
using TMPro;

public class UIGameplay : UIBase
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private TextMeshProUGUI scoreText;

    public static bool IsPaused { get; private set; }

    private void OnEnable()
    {
        if (pauseButton == null)
        {
            Debug.LogWarning("[UIGameplay] Pause button belum di-assign.", this);
            return;
        }

        pauseButton.onClick.AddListener(OpenPauseMenu);
    }

    private void OnDisable()
    {
        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(OpenPauseMenu);

        // Jaring pengaman: jangan sampai scene ditinggalkan dalam keadaan timeScale 0.
        if (IsPaused) ResumeGame();
    }

    public void OpenPauseMenu()
    {
        if (IsPaused) return;

        if (UIManager.Instance == null)
        {
            Debug.LogError("[UIGameplay] UIManager tidak ditemukan.", this);
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.PlayAudio(GameManager.Instance.ui_click);

        IsPaused = true;
        Time.timeScale = 0f;

        UIManager.Instance.ShowUI<UIPauseMenu>();
    }

    /// <summary>Panggil dari tombol Resume/Close di panel pause.</summary>
    public void ResumeGame()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else OpenPauseMenu();
    }

}