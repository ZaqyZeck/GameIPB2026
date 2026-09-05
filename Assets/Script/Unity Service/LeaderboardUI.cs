using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    [Header("List")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private RankBarUI rankBarPrefab;
    [SerializeField] private int maxEntriesPerSlide = 5;

    [Header("Controls")]
    [SerializeField] private Button addScoreButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button refreshButton;

    private const int TestScoreAmount = 10;

    [Header("Optional Info")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text playerRankText;
    [SerializeField] private TMP_Text currentSlideNumberText;

    private readonly List<RankBarUI> spawnedBars = new List<RankBarUI>();

    private int currentPage;
    private int totalEntries;
    private bool isLoading;

    private int TotalPages => totalEntries <= 0 ? 1 : Mathf.CeilToInt((float)totalEntries / maxEntriesPerSlide);

    private void OnEnable()
    {
        var leaderboard = LeaderboardManager.Instance;
        if (leaderboard == null) return;

        leaderboard.OnScoreAdded += HandleScoreAdded;
        leaderboard.OnError += ShowStatus;
    }

    private void OnDisable()
    {
        var leaderboard = LeaderboardManager.Instance;
        if (leaderboard == null) return;

        leaderboard.OnScoreAdded -= HandleScoreAdded;
        leaderboard.OnError -= ShowStatus;
    }

    private void Start()
    {
        if (addScoreButton != null)
            addScoreButton.onClick.AddListener(OnAddScoreClicked);

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);

        if (prevButton != null)
            prevButton.onClick.AddListener(OnPrevClicked);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(OnRefreshClicked);

        currentPage = 0;

        RefreshLeaderboard();
    }

    private async void OnAddScoreClicked()
    {
        if (isLoading) return;

        SetLoading(true);
        ShowStatus("Submitting score...");

        var entry = await LeaderboardManager.Instance.AddScoreAsync(TestScoreAmount);

        if (entry != null)
        {
            ShowStatus($"Score submitted! New total: {entry.Score}");
            await RefreshPlayerRank();
        }

        currentPage = 0;
        await RefreshLeaderboardAsync();

        SetLoading(false);
    }

    private async void OnRefreshClicked()
    {
        if (isLoading) return;

        SetLoading(true);

        currentPage = 0;

        await RefreshLeaderboardAsync();
        await RefreshPlayerRank();

        SetLoading(false);
    }

    private async void RefreshLeaderboard()
    {
        if (isLoading) return;

        SetLoading(true);
        ShowStatus("Loading leaderboard...");

        currentPage = 0;

        await RefreshLeaderboardAsync();
        await RefreshPlayerRank();

        SetLoading(false);
    }

    private async void OnNextClicked()
    {
        if (isLoading) return;
        if (!CanGoNext()) return;

        currentPage++;

        await LoadCurrentPage();
    }

    private async void OnPrevClicked()
    {
        if (isLoading) return;
        if (!CanGoPrevious()) return;

        currentPage--;

        await LoadCurrentPage();
    }

    private async Task LoadCurrentPage()
    {
        SetLoading(true);
        ShowStatus("Loading leaderboard...");

        await RefreshLeaderboardAsync();

        SetLoading(false);
    }

    private async Task RefreshLeaderboardAsync()
    {
        int offset = currentPage * maxEntriesPerSlide;

        LeaderboardPageResult result = await LeaderboardManager.Instance.GetTopScoresAsync(maxEntriesPerSlide, offset);

        totalEntries = result.Total;

        int maxPage = Mathf.Max(0, TotalPages - 1);

        if (currentPage > maxPage)
        {
            currentPage = maxPage;

            offset = currentPage * maxEntriesPerSlide;

            result = await LeaderboardManager.Instance.GetTopScoresAsync(maxEntriesPerSlide, offset);

            totalEntries = result.Total;
        }

        PopulateList(result.Entries);
        UpdatePaginationUI();
    }

    private async Task RefreshPlayerRank()
    {
        if (playerRankText == null) return;

        var myEntry = await LeaderboardManager.Instance.GetPlayerScoreAsync();

        playerRankText.text = myEntry != null
            ? $"Your rank: #{myEntry.Rank + 1}  (score: {myEntry.Score})"
            : "You haven't submitted a score yet.";
    }

    private void HandleScoreAdded(LeaderboardEntry entry)
    {
        ShowStatus($"Added {TestScoreAmount} points! Total: {entry.Score}");
    }

    private void PopulateList(List<LeaderboardEntry> entries)
    {
        ClearList();

        string myPlayerId = AuthenticationService.Instance != null
            ? AuthenticationService.Instance.PlayerId
            : null;

        foreach (var entry in entries)
        {
            RankBarUI bar = Instantiate(rankBarPrefab, contentParent);

            bar.RankNumber.text = (entry.Rank + 1).ToString();
            bar.Usename.text = string.IsNullOrEmpty(entry.PlayerName)
                ? entry.PlayerId
                : entry.PlayerName;
            bar.Score.text = entry.Score.ToString("0");

            if (!string.IsNullOrEmpty(myPlayerId) && entry.PlayerId == myPlayerId)
            {
                bar.Usename.text += " (You)";
            }

            spawnedBars.Add(bar);
        }

        if (entries.Count == 0)
        {
            ShowStatus("No scores yet. Be the first!");
        }
        else
        {
            ShowStatus(string.Empty);
        }
    }

    private void ClearList()
    {
        foreach (var bar in spawnedBars)
        {
            if (bar != null)
                Destroy(bar.gameObject);
        }

        spawnedBars.Clear();
    }

    private bool CanGoNext()
    {
        return currentPage < TotalPages - 1;
    }

    private bool CanGoPrevious()
    {
        return currentPage > 0;
    }

    private void UpdatePaginationUI()
    {
        if (prevButton != null)
            prevButton.interactable = !isLoading && CanGoPrevious();

        if (nextButton != null)
            nextButton.interactable = !isLoading && CanGoNext();

        if (currentSlideNumberText != null)
            currentSlideNumberText.text = $"{currentPage + 1} / {TotalPages}";
    }

    private void SetLoading(bool loading)
    {
        isLoading = loading;

        if (addScoreButton != null)
            addScoreButton.interactable = !loading;

        if (refreshButton != null)
            refreshButton.interactable = !loading;

        UpdatePaginationUI();
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}