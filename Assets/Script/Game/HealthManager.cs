using System;
using Ohm.UISystem;
using TMPro;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int health;
    public event Action<int, int> OnHealthChanged; 
    public event Action OnDeath;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI healthText;
    private void Awake()
    {
        Instance = this;
        DifficultyProfileSO activeProfile = null;
        if (GameManager.Instance != null && GameManager.Instance.SelectedDifficulty != null)
        {
            activeProfile = GameManager.Instance.SelectedDifficulty;
        }
        else if (PetManager.Instance != null && PetManager.Instance.DifficultyProfile != null)
        {
            activeProfile = PetManager.Instance.DifficultyProfile;
        }

        if (activeProfile != null)
        {
            maxHealth = activeProfile.targetStones;
        }
        else
        {
            maxHealth = 5;
        }
        health = maxHealth;
    }

    private void Start()
    {
        if (healthText != null) healthText.text = "Health: " + health;
        OnHealthChanged?.Invoke(health, health);
        GameEventBus.OnTakeDamage?.Invoke(health, health);
    }

    public void TakeDamage(int amount = 1)
    {
        if (health <= 0) return; 

        int before = health;
        health = Mathf.Clamp(health - Mathf.Abs(amount), 0, maxHealth);

        OnHealthChanged?.Invoke(before, health);
        GameEventBus.OnTakeDamage?.Invoke(before, health);
        if (healthText != null) healthText.text = "Health: " + health;
        if (health == 0)
        {
            OnDeath?.Invoke();
            GameEventBus.OnWin?.Invoke();
            UIManager.Instance.ShowUI<UIGameOver>();
            Debug.LogWarning("Menaaaaang");
        }
            
    }
    public int GetHealth() => health;

    public int GetMaxHealth() => maxHealth;

    public bool IsDead() => health <= 0;

    public void ResetHealth()
    {
        health = maxHealth;
    }
}