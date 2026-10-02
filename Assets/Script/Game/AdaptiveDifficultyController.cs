using System.Collections.Generic;
using UnityEngine;

public class AdaptiveDifficultyController : MonoBehaviour
{
    public static AdaptiveDifficultyController Instance { get; private set; }

    [SerializeField] private int currentLevel = 3;
    public int CurrentLevel => currentLevel;

    private int consecutiveSuccesses = 0;
    private int consecutiveFailures = 0;
    private readonly List<float> sessionSolveTimes = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ApplyLevel(3);
    }

    private void OnEnable()
    {
        GameEventBus.OnCustomerResolved += HandleCustomerResolved;
    }

    private void OnDisable()
    {
        GameEventBus.OnCustomerResolved -= HandleCustomerResolved;
        if (Instance == this) Instance = null;
    }

    private void HandleCustomerResolved(bool success, float solveDuration)
    {
        if (success)
        {
            consecutiveSuccesses++;
            consecutiveFailures = 0;
            if (solveDuration > 0f)
            {
                sessionSolveTimes.Add(solveDuration);
                if (sessionSolveTimes.Count > 8) sessionSolveTimes.RemoveAt(0);
            }

            EvaluateProgression(solveDuration);
        }
        else
        {
            consecutiveFailures++;
            consecutiveSuccesses = 0;

            EvaluateRelief();
        }
    }

    private void EvaluateProgression(float lastSolveDuration)
    {
        float avgSolve = GetRecentAverageSolveTime();

        if (currentLevel < 5)
        {
            bool shouldPromote = false;

            if (currentLevel == 1)
            {
                if (lastSolveDuration < 40f || consecutiveSuccesses >= 1)
                    shouldPromote = true;
            }
            else if (currentLevel == 2)
            {
                if (lastSolveDuration < 32f || consecutiveSuccesses >= 2)
                    shouldPromote = true;
            }
            else if (currentLevel == 3)
            {
                if ((consecutiveSuccesses >= 2 && lastSolveDuration < 28f) || consecutiveSuccesses >= 3)
                    shouldPromote = true;
            }
            else if (currentLevel == 4)
            {
                if ((consecutiveSuccesses >= 2 && lastSolveDuration < 22f) || (consecutiveSuccesses >= 3 && avgSolve < 26f))
                    shouldPromote = true;
            }

            if (shouldPromote)
            {
                ApplyLevel(currentLevel + 1);
            }
        }
    }

    private void EvaluateRelief()
    {
        if (currentLevel > 1)
        {
            if (consecutiveFailures >= 2)
            {
                ApplyLevel(1);
            }
            else
            {
                ApplyLevel(currentLevel - 1);
            }
        }
    }

    private float GetRecentAverageSolveTime()
    {
        if (sessionSolveTimes.Count == 0) return 30f;
        float sum = 0f;
        for (int i = 0; i < sessionSolveTimes.Count; i++) sum += sessionSolveTimes[i];
        return sum / sessionSolveTimes.Count;
    }

    public void ApplyLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, 5);

        if (PetManager.Instance != null && PetManager.Instance.DifficultyProfile != null)
        {
            var profile = PetManager.Instance.DifficultyProfile;
            switch (currentLevel)
            {
                case 1:
                    profile.maxActivePets = 4;
                    profile.extraPatienceTime = 45f;
                    profile.sameTypeSpawnChance = 0.15f;
                    profile.rollDislikes = false;
                    break;
                case 2:
                    profile.maxActivePets = 5;
                    profile.extraPatienceTime = 35f;
                    profile.sameTypeSpawnChance = 0.30f;
                    profile.rollDislikes = true;
                    break;
                case 3:
                    profile.maxActivePets = 6;
                    profile.extraPatienceTime = 30f;
                    profile.sameTypeSpawnChance = 0.40f;
                    profile.rollDislikes = true;
                    break;
                case 4:
                    profile.maxActivePets = 7;
                    profile.extraPatienceTime = 20f;
                    profile.sameTypeSpawnChance = 0.50f;
                    profile.rollDislikes = true;
                    break;
                case 5:
                    profile.maxActivePets = 8;
                    profile.extraPatienceTime = 15f;
                    profile.sameTypeSpawnChance = 0.65f;
                    profile.rollDislikes = true;
                    break;
            }
        }

        if (OwnerManager.Instance != null)
        {
            switch (currentLevel)
            {
                case 1: OwnerManager.Instance.SetSpawnInterval(14f, 18f); break;
                case 2: OwnerManager.Instance.SetSpawnInterval(12f, 16f); break;
                case 3: OwnerManager.Instance.SetSpawnInterval(10f, 14f); break;
                case 4: OwnerManager.Instance.SetSpawnInterval(8f, 12f); break;
                case 5: OwnerManager.Instance.SetSpawnInterval(7f, 10f); break;
            }
        }
    }
}
