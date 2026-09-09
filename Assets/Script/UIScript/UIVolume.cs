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

    void Start()
    {
        SetUpButton();
    }

    public void SetUpButton()
    {
        closeButton.onClick.AddListener(() =>
        {
            UIManager.Instance.OnEscape();
        });
    }
}
