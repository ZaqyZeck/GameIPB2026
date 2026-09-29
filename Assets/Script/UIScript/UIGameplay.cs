using UnityEngine;
using UnityEngine.UI;
using Ohm.UISystem;
using TMPro;

public class UIGameplay : UIBase
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button dropButton;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private SpiritStoneDisplay spiritStoneDisplay;

    //public static bool IsPaused { get; private set; }

    /// <summary>Reputasi terakhir. Dibaca UIGameOver saat game over.</summary>
    public static int Reputation { get; private set; }

    public static void ResetReputation() => Reputation = 0;

    private void OnEnable()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OpenPauseMenu);
        else
            Debug.LogWarning("[UIGameplay] Pause button belum di-assign.", this);

        if (dropButton != null)
        {
            dropButton.onClick.AddListener(OnDropButtonClicked);
            dropButton.gameObject.SetActive(false);
        }

        GameEventBus.OnTakeDamage += HandleTakeDamage;
        GameEventBus.OnReputationChange += HandleReputationChange;
    }

    private void Update()
    {
        if (dropButton != null)
        {
            bool holding = PlayerInteract.Instance != null && PlayerInteract.Instance.isHoldingObject;
            if (dropButton.gameObject.activeSelf != holding)
            {
                dropButton.gameObject.SetActive(holding);
            }
        }
    }

    private void OnDropButtonClicked()
    {
        if (GameInputManager.Instance != null)
        {
            GameInputManager.Instance.TriggerDrop();
        }
        else if (PlayerInteract.Instance != null)
        {
            PlayerInteract.Instance.DropHoldObject();
        }
    }

    private void OnDisable()
    {
        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(OpenPauseMenu);

        GameEventBus.OnTakeDamage -= HandleTakeDamage;
        GameEventBus.OnReputationChange -= HandleReputationChange;

        // Jaring pengaman: jangan sampai scene ditinggalkan dalam keadaan timeScale 0.
        if (LevelManager.Instance.IsPause) ResumeGame();
    }

    private void HandleTakeDamage(int before, int after)
    {
        if (spiritStoneDisplay != null)
            spiritStoneDisplay.SetCount(after);
    }

    private void HandleReputationChange(int before, int after)
    {
        Reputation = after;

        if (scoreText != null)
            scoreText.text = after.ToString();
    }

    public void OpenPauseMenu()
    {
        if (!LevelManager.Instance.IsPlaying)
        {
            Debug.LogWarning("sudah terpause");
            return;
        }

        if (UIManager.Instance == null)
        {
            Debug.LogError("[UIGameplay] UIManager tidak ditemukan.", this);
            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.PlayAudio(GameManager.Instance.ui_click);

        //IsPaused = true;
        //Time.timeScale = 0f;
        GameEventBus.OnPause?.Invoke();

        UIManager.Instance.ShowUI<UIPauseMenu>();
    }

    /// <summary>Panggil dari tombol Resume/Close di panel pause.</summary>
    public void ResumeGame()
    {
        if (LevelManager.Instance.IsPlaying) return;

        //IsPaused = false;
        GameEventBus.OnResume?.Invoke();
        //Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (LevelManager.Instance.IsPause) ResumeGame();
        else OpenPauseMenu();
    }
}