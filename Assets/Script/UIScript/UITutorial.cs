using UnityEngine;
using UnityEngine.UI;
using Ohm.UISystem;

public class UITutorial : UIBase
{
    [Header("References")]
    [SerializeField] private Button closeButton;

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
