using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

/// <summary>
/// Handles all communication with Unity Cloud Leaderboards:
/// - Adding/updating the signed-in player's score
/// - Fetching the top N scores
/// - Fetching the signed-in player's own rank/score
///
/// Requires the "Leaderboards" package (com.unity.services.leaderboards) and
/// that Unity Services is already initialized + the player is signed in
/// (see AuthenticationManager from the login/register setup).
///
/// Create the leaderboard itself in the Unity Cloud Dashboard
/// (LiveOps -> Leaderboards) and paste its Leaderboard ID below.
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("Leaderboard Config")]
    [Tooltip("Must match the Leaderboard ID configured in the Unity Cloud Dashboard.")]
    [SerializeField] private string leaderboardId = "global_highscore";

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = true;

    // ---- Events UI can subscribe to ----
    public event Action<List<LeaderboardEntry>> OnScoresLoaded;
    public event Action<LeaderboardEntry> OnScoreAdded;
    public event Action<LeaderboardEntry> OnPlayerScoreLoaded;
    public event Action<string> OnError;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ---------------------------------------------------------------
    //  ADD SCORE
    // ---------------------------------------------------------------

    /// <summary>
    /// Submits a score for the currently signed-in player.
    /// Unity Leaderboards keeps the player's best score by default
    /// (configurable as Best/Last/etc. per-leaderboard in the dashboard).
    /// </summary>
    public async Task<LeaderboardEntry> AddScoreAsync(double score)
    {
        if (!EnsureSignedIn()) return null;

        try
        {
            await EnsurePlayerNameIsSetAsync();

            LeaderboardEntry entry = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
            Log($"Score submitted: {entry.PlayerName} -> {entry.Score} (rank {entry.Rank})");
            OnScoreAdded?.Invoke(entry);
            return entry;
        }
        catch (LeaderboardsException ex)
        {
            LogError($"AddScoreAsync failed (Leaderboards): {ex.Reason} - {ex.Message}");
            OnError?.Invoke(FriendlyLeaderboardError(ex));
        }
        catch (Exception ex)
        {
            LogError($"AddScoreAsync failed: {ex.Message}");
            OnError?.Invoke("Could not submit your score. Please try again.");
        }

        return null;
    }

    // ---------------------------------------------------------------
    //  GET TOP SCORES
    // ---------------------------------------------------------------

    /// <summary>
    /// Fetches the top <paramref name="limit"/> entries of the leaderboard,
    /// starting at <paramref name="offset"/> (0 = top of the board).
    /// </summary>
    public async Task<LeaderboardPageResult> GetTopScoresAsync(int limit = 50, int offset = 0)
    {
        if (!EnsureSignedIn()) return new LeaderboardPageResult(new List<LeaderboardEntry>(), 0);

        try
        {
            var options = new GetScoresOptions { Offset = offset, Limit = limit };
            LeaderboardScoresPage scoresPage = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, options);

            Log($"Fetched {scoresPage.Results.Count} leaderboard entries. Offset: {offset}, Total: {scoresPage.Total}");

            OnScoresLoaded?.Invoke(scoresPage.Results);

            return new LeaderboardPageResult(scoresPage.Results, scoresPage.Total);
        }
        catch (LeaderboardsException ex)
        {
            LogError($"GetTopScoresAsync failed (Leaderboards): {ex.Reason} - {ex.Message}");
            OnError?.Invoke(FriendlyLeaderboardError(ex));
        }
        catch (Exception ex)
        {
            LogError($"GetTopScoresAsync failed: {ex.Message}");
            OnError?.Invoke("Could not load the leaderboard. Please try again.");
        }

        return new LeaderboardPageResult(new List<LeaderboardEntry>(), 0);
    }

    // ---------------------------------------------------------------
    //  GET CURRENT PLAYER'S OWN SCORE/RANK
    // ---------------------------------------------------------------

    /// <summary>
    /// Fetches the signed-in player's own entry (rank + score), even if
    /// they're outside the top N returned by GetTopScoresAsync.
    /// Returns null if the player hasn't submitted a score yet.
    /// </summary>
    public async Task<LeaderboardEntry> GetPlayerScoreAsync()
    {
        if (!EnsureSignedIn()) return null;

        try
        {
            LeaderboardEntry entry = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
            OnPlayerScoreLoaded?.Invoke(entry);
            return entry;
        }
        catch (LeaderboardsException ex) when (ex.Reason == LeaderboardsExceptionReason.EntryNotFound)
        {
            Log("Player has no score on this leaderboard yet.");
            return null;
        }
        catch (LeaderboardsException ex)
        {
            LogError($"GetPlayerScoreAsync failed (Leaderboards): {ex.Reason} - {ex.Message}");
            OnError?.Invoke(FriendlyLeaderboardError(ex));
        }
        catch (Exception ex)
        {
            LogError($"GetPlayerScoreAsync failed: {ex.Message}");
            OnError?.Invoke("Could not load your score. Please try again.");
        }

        return null;
    }

    // ---------------------------------------------------------------
    //  HELPERS
    // ---------------------------------------------------------------

    private bool EnsureSignedIn()
    {
        if (AuthenticationService.Instance == null || !AuthenticationService.Instance.IsSignedIn)
        {
            LogError("Not signed in - cannot access the leaderboard.");
            OnError?.Invoke("You must be logged in to use the leaderboard.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Leaderboard entries show the UGS "player name", which is separate from
    /// the username/password login. If it hasn't been set yet, this sets it
    /// once so entries display something readable instead of a blank name.
    /// </summary>
    private async Task EnsurePlayerNameIsSetAsync()
    {
        try
        {
            string currentName = await AuthenticationService.Instance.GetPlayerNameAsync();

            if (string.IsNullOrEmpty(currentName) || currentName.StartsWith(AuthenticationService.Instance.PlayerId.Substring(0, 5)))
            {
                string desiredName = AuthenticationManager.Instance != null && !string.IsNullOrEmpty(AuthenticationManager.Instance.PlayerName)
                    ? AuthenticationManager.Instance.PlayerName
                    : $"Player{UnityEngine.Random.Range(1000, 9999)}";

                await AuthenticationService.Instance.UpdatePlayerNameAsync(desiredName);
                Log($"Player name set to '{desiredName}'.");
            }
        }
        catch (Exception ex)
        {
            // Non-fatal: if this fails, the score submission can still proceed.
            LogError($"Could not set player name: {ex.Message}");
        }
    }

    private string FriendlyLeaderboardError(LeaderboardsException ex)
    {
        return ex.Reason switch
        {
            LeaderboardsExceptionReason.NotFound => "Leaderboard not found. Check the Leaderboard ID.",
            LeaderboardsExceptionReason.InvalidArgument => "Invalid score or request data.",
            _ => "Something went wrong with the leaderboard. Please try again."
        };
    }

    private void Log(string message)
    {
        if (logDebugMessages) Debug.Log($"[Leaderboard] {message}");
    }

    private void LogError(string message)
    {
        if (logDebugMessages) Debug.LogError($"[Leaderboard] {message}");
    }
}