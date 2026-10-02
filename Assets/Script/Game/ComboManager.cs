using System;
using UnityEngine;

public enum ComboType
{
    ConsecutiveStreak, // Increments per successful customer, resets on mistake/timeout
    TimedCombo         // Has a countdown timer window that must be refreshed
}

/// <summary>
/// Groundwork foundation for the Furgetful Combo System.
/// Kept unlinked from live gameplay by default until game design decisions are finalized.
/// Can be enabled via inspector toggle or by subscribing to GameEventBus when ready.
/// </summary>
public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }

    [Header("Activation")]
    [Tooltip("Master toggle. If false, combo calculations return 1x and streak stays inactive.")]
    [SerializeField] private bool isEnabled = false;

    [Tooltip("If true, automatically hooks into GameEventBus.OnCustomerResolved. Leave false for manual control.")]
    [SerializeField] private bool autoListenToEventBus = false;

    [Header("Combo Rules")]
    [SerializeField] private ComboType comboType = ComboType.ConsecutiveStreak;
    [Tooltip("Time in seconds to maintain the combo if TimedCombo mode is used.")]
    [SerializeField] private float comboWindowSeconds = 18f;

    [Header("Multiplier Scaling")]
    [SerializeField] private float baseMultiplier = 1.0f;
    [Tooltip("Multiplier increase per combo streak (e.g. 0.2f means +20% per streak: 1.0x -> 1.2x -> 1.4x ...).")]
    [SerializeField] private float multiplierStep = 0.2f;
    [SerializeField] private float maxMultiplier = 2.0f;

    [Header("Fast Solve Bonus (Optional)")]
    [Tooltip("If solve duration is below this threshold (in seconds), an additional bonus is granted.")]
    [SerializeField] private float fastSolveThreshold = 18f;
    [SerializeField] private float fastSolveBonus = 0.1f;

    [Header("Runtime State")]
    [SerializeField] private int currentStreak = 0;
    [SerializeField] private float currentMultiplier = 1.0f;
    [SerializeField] private int highestStreak = 0;
    [SerializeField] private float timerRemaining = 0f;

    public bool IsEnabled => isEnabled;
    public int CurrentStreak => currentStreak;
    public float CurrentMultiplier => isEnabled ? currentMultiplier : 1.0f;
    public int HighestStreak => highestStreak;
    public float TimerProgress => comboWindowSeconds > 0 ? Mathf.Clamp01(timerRemaining / comboWindowSeconds) : 0f;

    // Events for UI and Audio feedback
    public static event Action<int, float> OnComboChanged; // (currentStreak, currentMultiplier)
    public static event Action<int> OnComboBroken;         // (streakBeforeBreak)
    public static event Action OnComboReset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (autoListenToEventBus)
        {
            GameEventBus.OnCustomerResolved += HandleCustomerResolved;
        }
    }

    private void OnDisable()
    {
        if (autoListenToEventBus)
        {
            GameEventBus.OnCustomerResolved -= HandleCustomerResolved;
        }
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!isEnabled || comboType != ComboType.TimedCombo || currentStreak <= 0)
            return;

        if (timerRemaining > 0f)
        {
            timerRemaining -= Time.deltaTime;
            if (timerRemaining <= 0f)
            {
                timerRemaining = 0f;
                RegisterBreak("Combo timer expired");
            }
        }
    }

    private void HandleCustomerResolved(bool success, float solveDuration)
    {
        if (!isEnabled) return;

        if (success)
        {
            RegisterSuccess(solveDuration);
        }
        else
        {
            RegisterBreak("Customer unsatisfied / timed out");
        }
    }

    /// <summary>
    /// Registers a successful customer reunion to advance the combo.
    /// </summary>
    public void RegisterSuccess(float solveDuration = 0f)
    {
        if (!isEnabled) return;

        currentStreak++;
        if (currentStreak > highestStreak)
        {
            highestStreak = currentStreak;
        }

        // Calculate multiplier
        float mult = baseMultiplier + (currentStreak - 1) * multiplierStep;

        // Apply fast solve bonus if eligible
        if (solveDuration > 0f && solveDuration <= fastSolveThreshold)
        {
            mult += fastSolveBonus;
        }

        currentMultiplier = Mathf.Min(mult, maxMultiplier);

        if (comboType == ComboType.TimedCombo)
        {
            timerRemaining = comboWindowSeconds;
        }

        OnComboChanged?.Invoke(currentStreak, currentMultiplier);
    }

    /// <summary>
    /// Breaks the active streak (e.g. wrong pet given, owner left angry).
    /// </summary>
    public void RegisterBreak(string reason = "")
    {
        if (!isEnabled || currentStreak <= 0) return;

        int brokenStreak = currentStreak;
        currentStreak = 0;
        currentMultiplier = baseMultiplier;
        timerRemaining = 0f;

        OnComboBroken?.Invoke(brokenStreak);
        OnComboChanged?.Invoke(0, baseMultiplier);
    }

    /// <summary>
    /// Fully resets the combo state (e.g. on new game / scene restart).
    /// </summary>
    public void ResetCombo()
    {
        currentStreak = 0;
        currentMultiplier = baseMultiplier;
        highestStreak = 0;
        timerRemaining = 0f;

        OnComboReset?.Invoke();
        OnComboChanged?.Invoke(0, baseMultiplier);
    }

    /// <summary>
    /// Multiplies a base score or reputation reward by the current combo multiplier.
    /// </summary>
    public int ApplyMultiplier(int baseScore)
    {
        if (!isEnabled || currentStreak <= 1) return baseScore;
        return Mathf.RoundToInt(baseScore * currentMultiplier);
    }

    /// <summary>
    /// Helper to get an ascending audio pitch based on current streak for dynamic sound effects.
    /// </summary>
    public float GetPitch(float startPitch = 1.0f, float pitchStep = 0.05f, float maxPitch = 1.4f)
    {
        if (currentStreak <= 1) return startPitch;
        return Mathf.Min(startPitch + (currentStreak - 1) * pitchStep, maxPitch);
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
        if (!isEnabled)
        {
            ResetCombo();
        }
    }

    [ContextMenu("Test: Add Streak (+1)")]
    private void TestAddStreak() => RegisterSuccess(10f);

    [ContextMenu("Test: Break Streak")]
    private void TestBreakStreak() => RegisterBreak("Manual debug break");
}
