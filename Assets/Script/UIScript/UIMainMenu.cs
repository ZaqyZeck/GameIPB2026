using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Ohm.UISystem;
public class UIMainMenu : UIBase
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button volumeButton;
    [SerializeField] private Button creditButton;
    [SerializeField] private Button tutorialButton;

    void Start()
    {
        SetUpButton();
    }
    public void SetUpButton()
    {
        startButton.onClick.AddListener(() =>
        {
            // GameManager.Instance.LoadScene(SceneType.Gameplay);
            Debug.Log("Start Game Clicked");
        });
        volumeButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowUI<UIVolume>();
        });
        creditButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowUI<UICredit>();
        });
        tutorialButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ShowUI<UITutorial>();
        });
        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
