using System.Collections.Generic;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

public class LeaderboardPageResult
{
    public List<LeaderboardEntry> Entries { get; }
    public int Total { get; }

    public LeaderboardPageResult(List<LeaderboardEntry> entries, int total)
    {
        Entries = entries;
        Total = total;
    }
}
