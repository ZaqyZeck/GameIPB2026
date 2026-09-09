using UnityEngine;
using UnityEngine.UI;
using Ohm.UISystem;
using TMPro;

public class UIGameOver : UIBase
{
    [Header("References")]
    [SerializeField] private Button saveButton;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI scoreText;

    void Awake()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveButton);
    }

    private void SaveButton()
    {
        
    }
}
