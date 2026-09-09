using UnityEngine;
using UnityEngine.UI;
using Ohm.UISystem;

public class UIVolume : UIBase
{
    [Header("References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Slider generalVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;


    private bool _isSyncing;

    private void Start()
    {
        SetupSlider();

        if (closeButton != null)
        {
            GameManager.Instance.PlayAudio(GameManager.Instance.ui_click);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseButton);
        }
    }

    private void OnEnable()
    {
        // Nilai bisa berubah dari tempat lain selagi panel tertutup.
        SyncSliders();
    }

    private void SetupSlider()
    {
        SetupRange(generalVolumeSlider);
        SetupRange(musicVolumeSlider);
        SetupRange(sfxVolumeSlider);

        SyncSliders();
    }

    private void SetupRange(Slider slider)
    {
        if (slider == null) return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void SyncSliders()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogWarning("[UISettings] GameManager tidak ditemukan.", this);
            return;
        }

        _isSyncing = true;

        SetSliderValue(generalVolumeSlider, gm.MasterVolume, OnMasterChanged);
        SetSliderValue(musicVolumeSlider, gm.BGMVolume, OnBGMChanged);
        SetSliderValue(sfxVolumeSlider, gm.SFXVolume, OnSFXChanged);

        _isSyncing = false;
    }

    private void SetSliderValue(
        Slider slider, float value, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null) return;

        slider.onValueChanged.RemoveAllListeners();
        slider.value = value;
        slider.onValueChanged.AddListener(callback);
    }

    // ---------- Slider callbacks ----------

    private void OnMasterChanged(float value)
    {
        if (_isSyncing) return;
        GameEventBus.onValueChangeMaster?.Invoke(value);
    }

    private void OnBGMChanged(float value)
    {
        if (_isSyncing) return;
        GameEventBus.onValueChangeBGM?.Invoke(value);
    }

    private void OnSFXChanged(float value)
    {
        if (_isSyncing) return;
        GameEventBus.onValueChangeSFX?.Invoke(value);
    }

    public void CloseButton()
    {
        UIManager.Instance.OnEscape();
    }
}
