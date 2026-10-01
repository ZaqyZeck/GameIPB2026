using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance;

    [System.Serializable]
    public class ReputationEntry
    {
        public int delta;
        public string reason;
        public string timestamp;
        public int resultingScore;
    }
    

    [Header("Score Settings")]
    [SerializeField] private int startingScore = 0;
    [SerializeField] private int minScore = -100;
    [SerializeField] private int maxScore = 10000000;

    [SerializeField] private int score;
    private List<ReputationEntry> history = new List<ReputationEntry>();
    public event Action<int, int> OnReputationChanged; 
    
    [Header("UI")]
    [SerializeField] TextMeshProUGUI reputationText;

    private void Awake()
    {
        Instance = this;
        score = startingScore;
    }

    private void Start()
    {
        UpdateUI(score);
        GameEventBus.OnReputationChange?.Invoke(score, score);
    }

    public int Adjust(int delta, string reason = "No reason given")
    {
        return Adjust(delta, Vector3.zero, reason);
    }

    public void UpdateUI(int newScore)
    {
        if (reputationText != null)
            reputationText.text = "Reputation: " + newScore;
    }

    public int Adjust(int delta, Vector3 sourceWorldPos, string reason = "No reason given")
    {
        int before = score;
        score = Mathf.Clamp(score + delta, minScore, maxScore);

        history.Add(new ReputationEntry
        {
            delta = delta,
            reason = reason,
            timestamp = DateTime.UtcNow.ToString("o"),
            resultingScore = score
        });

        OnReputationChanged?.Invoke(before, score);

        if (sourceWorldPos != Vector3.zero)
        {
            GameEventBus.OnReputationDeltaWorld?.Invoke(delta, score, sourceWorldPos);
        }
        else
        {
            UpdateUI(score);
            GameEventBus.OnReputationChange?.Invoke(before, score);
        }

        return score;
    }

    public int Reward(int amount, string reason = "")
        => Adjust(Mathf.Abs(amount), reason);

    public int Reward(int amount, Vector3 sourceWorldPos, string reason = "")
        => Adjust(Mathf.Abs(amount), sourceWorldPos, reason);

    public int Penalize(int amount, string reason = "")
        => Adjust(-Mathf.Abs(amount), reason);

    public int Penalize(int amount, Vector3 sourceWorldPos, string reason = "")
        => Adjust(-Mathf.Abs(amount), sourceWorldPos, reason);

    public int GetScore() => score;

    public List<ReputationEntry> GetHistory() => history;

    public void ResetReputation()
    {
        score = startingScore;
        history.Clear();
        UpdateUI(score);
    }

    //private int CalculateScore(ScoreVariable scoreVariable)
    //{
    //    int calculatedScore = 0;
    //    return calculatedScore;
    //}
}

//public class ScoreVariable
//{
//    public float currentPatience;
//    public float maxPatience;
//    public int descriptionCounter;
//}