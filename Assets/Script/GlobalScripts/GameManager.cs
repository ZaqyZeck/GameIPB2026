using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using PixeLadder.EasyTransition;
using Ami.BroAudio;
using Ohm.UISystem;

[Serializable]
public class SceneEntry
{
    public SceneType type;
    public SceneReference scene;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scenes")]
    [Tooltip("Drag scene asset ke sini. Jangan lupa daftarkan juga di Build Settings.")]
    [SerializeField] private SceneEntry[] _scenes;

    [Header("Transitions")]
    [SerializeField] private TransitionEffect _defaultTransition;
    [SerializeField] private TransitionEffect _restartTransition;

    [Header("Audio")]
    public SoundID bgmMainMenu;
    public SoundID bgmGameplay;
    public SoundID ui_click;
    public SoundID meow;
    public SoundID meowAngry;
    public SoundID meowSleep;
    public SoundID dog;
    public SoundID dogAngry;
    public SoundID dogSleep;
    public SoundID lonceng;
    public SoundID hover;
    public SoundID clickUI;
    public SoundID correctNumberPopup;
    public SoundID numberBumpCorrect;
    public SoundID dialogue;
    public SoundID burn;

    [Range(0f, 1f)] public float defaultMaster = 0.5f;
    [Range(0f, 1f)] public float defaultBGM = 0.5f;
    [Range(0f, 1f)] public float defaultSFX = 0.5f;
    [Tooltip("Durasi crossfade saat ganti BGM antar scene.")]
    [SerializeField] private float _bgmFadeTime = 0.5f;

    [Header("Difficulty")]
    [SerializeField] private DifficultyProfileSO defaultDifficulty;
    [SerializeField] private DifficultyProfileSO easyDifficulty;
    [SerializeField] private DifficultyProfileSO mediumDifficulty;
    [SerializeField] private DifficultyProfileSO adaptiveDifficulty;

    public DifficultyProfileSO SelectedDifficulty { get; private set; }

    // ---------- Runtime state ----------

    public float MasterVolume { get; private set; }
    public float BGMVolume { get; private set; }
    public float SFXVolume { get; private set; }

    private bool _bgmPlaying;
    private bool _gameplayBgmPlaying;
    private bool _isLoadingScene;
    private GameObject _lastHoveredUI;
    private readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>();

    // ---------- Lifecycle ----------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (SelectedDifficulty == null)
        {
            SelectedDifficulty = defaultDifficulty != null ? defaultDifficulty : easyDifficulty;
        }
        ApplyDefaultVolume();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneTransitioner.OnScreenCovered += HandleScreenCovered;
    }

    private void OnEnable()
    {
        // Instance duplikat sudah dijadwalkan destroy — jangan ikut subscribe.
        if (Instance != this) return;

        GameEventBus.onValueChangeMaster += SetMasterVolume;
        GameEventBus.onValueChangeBGM += SetBGMVolume;
        GameEventBus.onValueChangeSFX += SetSFXVolume;
    }

    private void OnDisable()
    {
        if (Instance != this) return;

        GameEventBus.onValueChangeMaster -= SetMasterVolume;
        GameEventBus.onValueChangeBGM -= SetBGMVolume;
        GameEventBus.onValueChangeSFX -= SetSFXVolume;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneTransitioner.OnScreenCovered -= HandleScreenCovered;
            Instance = null;
        }
    }

    void OnValidate()
    {
        if (_scenes == null) return;

        foreach (var entry in _scenes)
            entry?.scene?.Sync();

        // Peringatkan kalau ada SceneType dobel — GetScene cuma pakai yang pertama.
        for (int i = 0; i < _scenes.Length; i++)
        {
            if (_scenes[i] == null) continue;

            for (int j = i + 1; j < _scenes.Length; j++)
            {
                if (_scenes[j] != null && _scenes[j].type == _scenes[i].type)
                    Debug.LogWarning(
                        $"[GameManager] SceneType '{_scenes[i].type}' terdaftar lebih dari sekali.", this);
            }
        }
    }

    void Update()
    {
        UpdateUIHover();
        UpdateUIClick();
    }

    private void UpdateUIHover()
    {
        if (EventSystem.current == null) return;

        if (GameInputManager.Instance != null && !GameInputManager.Instance.IsPointerOverUI())
        {
            _lastHoveredUI = null;
            return;
        }

        Vector2 pointerPos = GameInputManager.Instance != null
            ? GameInputManager.Instance.CurrentPointerPosition
            : (Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero);

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = pointerPos
        };

        _uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, _uiRaycastResults);

        GameObject hoveredUI = null;
        for (int i = 0; i < _uiRaycastResults.Count; i++)
        {
            var go = _uiRaycastResults[i].gameObject;
            if (go == null) continue;

            var selectable = go.GetComponentInParent<Selectable>();
            if (selectable != null && selectable.isActiveAndEnabled && selectable.interactable)
            {
                hoveredUI = selectable.gameObject;
                break;
            }
        }

        if (hoveredUI != _lastHoveredUI)
        {
            _lastHoveredUI = hoveredUI;
            if (hoveredUI != null)
            {
                PlayAudio(hover);
            }
        }
    }

    private void UpdateUIClick()
    {
        if (EventSystem.current == null) return;

        bool isPointerPressed = Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
        bool isGamepadPressed = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (!isPointerPressed && !isGamepadPressed) return;

        if (isPointerPressed)
        {
            bool isOverUI = GameInputManager.Instance != null
                ? GameInputManager.Instance.IsPointerOverUI()
                : EventSystem.current.IsPointerOverGameObject();

            if (!isOverUI) return;

            Vector2 pointerPos = GameInputManager.Instance != null
                ? GameInputManager.Instance.CurrentPointerPosition
                : (Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero);

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = pointerPos
            };

            _uiRaycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, _uiRaycastResults);

            for (int i = 0; i < _uiRaycastResults.Count; i++)
            {
                var go = _uiRaycastResults[i].gameObject;
                if (go == null) continue;

                var selectable = go.GetComponentInParent<Selectable>();
                if (selectable != null && selectable.isActiveAndEnabled && selectable.interactable)
                {
                    PlayAudio(clickUI.IsValid() ? clickUI : ui_click);
                    break;
                }
            }
        }
        else if (isGamepadPressed)
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                var selectable = EventSystem.current.currentSelectedGameObject.GetComponentInParent<Selectable>();
                if (selectable != null && selectable.isActiveAndEnabled && selectable.interactable)
                {
                    PlayAudio(clickUI.IsValid() ? clickUI : ui_click);
                }
            }
        }
    }

    private void HandleScreenCovered()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllUI(instant: true);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isLoadingScene = false;
        _lastHoveredUI = null;

        if (IsScene(scene, SceneType.MainMenu))
        {
            UIGameplay.ResetReputation();
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseAllUI(instant: true);
                UIManager.Instance.ShowUI<UIMainMenu>(showInstant: true, hideCurrentInstant: true);
            }
        }
        else if (IsScene(scene, SceneType.Gameplay))
        {
            UIGameplay.ResetReputation();
            if (UIManager.Instance != null)
            {
                UIManager.Instance.CloseAllUI(instant: true);
                UIManager.Instance.ShowUI<UIGameplay>(showInstant: true, hideCurrentInstant: true);
            }
            if (UIGameplay.Instance != null)
            {
                UIGameplay.Instance.RefreshDisplay();
            }
            var stones = FindObjectsByType<SpiritStoneDisplay>(FindObjectsSortMode.None);
            foreach (var stone in stones)
            {
                stone.ResetDisplay();
            }
        }

        // Gameplay pakai BGM sendiri, scene lain pakai BGM main menu.
        ApplyBGMFor(IsScene(scene, SceneType.Gameplay));
    }

    private bool IsScene(Scene scene, SceneType type)
    {
        var reference = GetScene(type);
        return reference != null && reference.IsValid && scene.path == reference.Path;
    }

    // ---------- Scene loading ----------

    private SceneReference GetScene(SceneType type)
    {
        if (_scenes == null) return null;

        foreach (var entry in _scenes)
            if (entry != null && entry.type == type)
                return entry.scene;

        return null;
    }

    public void LoadScene(SceneType type) => LoadScene(type, _defaultTransition);

    public void LoadScene(SceneType type, TransitionEffect effect)
    {
        var scene = GetScene(type);
        if (scene == null || !scene.IsValid)
        {
            Debug.LogError($"[GameManager] Scene '{type}' belum di-assign.", this);
            return;
        }

        LoadScenePath(scene.Path, effect);
    }

    public void LoadMainMenu()
    {
        LoadScene(SceneType.MainMenu);
    }

    public void RestartScene()
    {
        LoadScenePath(
            SceneManager.GetActiveScene().path,
            _restartTransition != null ? _restartTransition : _defaultTransition);
    }

    private void LoadScenePath(string path, TransitionEffect effect)
    {
        if (_isLoadingScene)
        {
            Debug.LogWarning("[GameManager] Scene sedang dimuat, permintaan diabaikan.", this);
            return;
        }

        _isLoadingScene = true;

        // Pause bisa menyisakan timeScale 0 — kembalikan sebelum pindah scene.
        Time.timeScale = 1f;

        if (SceneTransitioner.Instance != null)
        {
            SceneTransitioner.Instance.LoadScene(path, effect);
        }
        else
        {
            Debug.LogWarning(
                "[GameManager] SceneTransitioner tidak ditemukan. Fallback tanpa transisi.", this);
            SceneManager.LoadScene(path);
        }
    }

    // ---------- Audio ----------

    private void ApplyDefaultVolume()
    {
        SetMasterVolume(defaultMaster);
        SetBGMVolume(defaultBGM);
        SetSFXVolume(defaultSFX);
    }

    /// <summary>
    /// Pastikan BGM yang sesuai sedang jalan. Kalau BGM-nya sudah benar,
    /// dibiarkan terus supaya tidak restart tiap pindah scene.
    /// </summary>
    private void ApplyBGMFor(bool isGameplay)
    {
        if (_bgmPlaying && _gameplayBgmPlaying == isGameplay) return;

        if (_bgmPlaying)
            BroAudio.Stop(BroAudioType.Music, _bgmFadeTime);

        PlayAudio(isGameplay ? bgmGameplay : bgmMainMenu);

        _bgmPlaying = true;
        _gameplayBgmPlaying = isGameplay;
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = value;
        BroAudio.SetVolume(BroAudioType.All, value);
    }

    public void SetBGMVolume(float value)
    {
        BGMVolume = value;
        BroAudio.SetVolume(BroAudioType.Music, value);
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = value;
        BroAudio.SetVolume(BroAudioType.SFX, value);
    }

    public void PlayAudio(SoundID audio) => BroAudio.Play(audio);

    public void StopAudio()
    {
        BroAudio.Stop(BroAudioType.Music);
        _bgmPlaying = false;
    }

    public void SetDifficulty(DifficultyTier tier)
    {
        switch (tier)
        {
            case DifficultyTier.Easy:
                SelectedDifficulty = easyDifficulty != null ? easyDifficulty : defaultDifficulty;
                break;
            case DifficultyTier.Medium:
                SelectedDifficulty = mediumDifficulty != null ? mediumDifficulty : defaultDifficulty;
                break;
            case DifficultyTier.Adaptive:
                SelectedDifficulty = adaptiveDifficulty != null ? adaptiveDifficulty : defaultDifficulty;
                break;
        }
    }

    public void SetDifficulty(DifficultyProfileSO profile)
    {
        SelectedDifficulty = profile;
    }
}