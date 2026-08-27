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

    public int Adjust(int delta, string reason = "No reason given")
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

        if(reputationText != null) reputationText.text = "Reputation: " + score;
        OnReputationChanged?.Invoke(before, score);

        return score;
    }

    public int Reward(int amount, string reason = "")
        => Adjust(Mathf.Abs(amount), reason);

    public int Penalize(int amount, string reason = "")
        => Adjust(-Mathf.Abs(amount), reason);

    public int GetScore() => score;

    public List<ReputationEntry> GetHistory() => history;

    public void ResetReputation()
    {
        score = startingScore;
        history.Clear();
    }

}