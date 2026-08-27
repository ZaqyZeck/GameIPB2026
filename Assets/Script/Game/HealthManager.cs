using System;
using TMPro;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int health;
    public event Action<int, int> OnHealthChanged; 
    public event Action OnDeath;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI healthText;
    private void Awake()
    {
        Instance = this;
        health = maxHealth;
    }

    public void TakeDamage(int amount = 1)
    {
        if (health <= 0) return; 

        int before = health;
        health = Mathf.Clamp(health - Mathf.Abs(amount), 0, maxHealth);

        OnHealthChanged?.Invoke(before, health);
        if (healthText != null) healthText.text = "Health: " + health;
        if (health == 0)
            OnDeath?.Invoke();
    }
    public int GetHealth() => health;

    public int GetMaxHealth() => maxHealth;

    public bool IsDead() => health <= 0;

    public void ResetHealth()
    {
        health = maxHealth;
    }
}