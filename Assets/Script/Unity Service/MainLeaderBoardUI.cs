using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using DG.Tweening;

public class MainLeaderBoardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject LeaderboardParent;
    public bool isOverLeaderboard { get; private set; } = false;

    [SerializeField] private int scrollMultiplier = 40;
    [SerializeField] private float maxAbove = 290f;
    [SerializeField] private float maxBellow = -290f;

    [Header("Snap Back Animation")]
    [Tooltip("Durasi animasi kembali ke batas atas/bawah.")]
    [SerializeField] private float snapDuration = 0.35f;
    [SerializeField] private Ease snapEase = Ease.OutCubic;

    [Header("List")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private RankBarUI rankBarPrefab;
    [SerializeField] private int topEntriesCount = 200;

    [Header("Optional Info")]
    [SerializeField] private TMP_Text statusText;

    [Header("Anonymous Login")]
    [Tooltip("Nama pemain yang dipakai jika belum login, hanya untuk melihat leaderboard.")]
    [SerializeField] private string anonymousViewerName = "Viewer";

    [Header("Debug")]
    [SerializeField] private bool logDebugMessages = true;

    private readonly List<RankBarUI> spawnedBars = new List<RankBarUI>();
    private bool isLoading;

    // ---- Snap-back state ----
    private Tween snapTween;
    private bool isSnapping;

    private void Update()
    {
        if (isOverLeaderboard)
        {
            Vector2 scrollDelta = Mouse.current.scroll.ReadValue();

            float scrollY = scrollDelta.y;
            ScrollLeaderboard(scrollY);
        }
    }

    // Triggered automatically when the mouse enters the UI Image bounds
    public void OnPointerEnter(PointerEventData eventData)
    {
        isOverLeaderboard = true;
    }

    // Triggered automatically when the mouse leaves the UI Image bounds
    public void OnPointerExit(PointerEventData eventData)
    {
        isOverLeaderboard = false;
    }

    private void ScrollLeaderboard(float scrollY)
    {
        // Selama animasi snap-back berjalan, abaikan input scroll manual
        // supaya contentParent tidak "ditarik" dua arah sekaligus.
        if (isSnapping) return;
        if (Mathf.Approximately(scrollY, 0f)) return;

        contentParent.localPosition += new Vector3(0f, -(scrollY * scrollMultiplier), 0f);

        CheckScrollBounds();
    }

    // ---------------------------------------------------------------
    //  BOUNDARY CHECK + SNAP BACK
    // ---------------------------------------------------------------

    /// <summary>
    /// Mengecek posisi bar paling atas (rank #1) dan bar paling bawah
    /// terhadap world/canvas position Y = 0. Jika melewati batas,
    /// animasikan contentParent kembali ke maxAbove / maxBellow.
    /// </summary>
    private void CheckScrollBounds()
    {
        if (spawnedBars.Count == 0) return;

        RankBarUI topBar = spawnedBars[0];
        RankBarUI bottomBar = spawnedBars[spawnedBars.Count - 1];

        if (topBar == null || bottomBar == null) return;

        // Bar rank #1 turun di bawah world position 0 -> overscroll ke atas
        if (topBar.transform.position.y < 500f)
        {
            SnapTo(maxAbove);
        }
        // Bar paling bawah naik di atas world position 0 -> overscroll ke bawah
        else if (bottomBar.transform.position.y > 500f)
        {
            SnapTo(bottomBar.transform.position.y + maxBellow);
        }
    }

    private void SnapTo(float targetLocalY)
    {
        // Kalau sudah snapping ke arah yang sama, tidak perlu restart tween.
        if (isSnapping && Mathf.Approximately(contentParent.localPosition.y, targetLocalY))
            return;

        isSnapping = true;
        snapTween?.Kill();

        snapTween = contentParent
            .DOLocalMoveY(targetLocalY, snapDuration)
            .SetEase(snapEase)
            .OnComplete(() => isSnapping = false)
            .OnKill(() => isSnapping = false);
    }

    private void OnEnable()
    {
        LoadLeaderboard();
    }

    private void OnDisable()
    {
        isOverLeaderboard = false;

        // Hentikan tween yang mungkin masih berjalan saat object dinonaktifkan.
        snapTween?.Kill();
        isSnapping = false;
    }

    private void Start()
    {
        LoadLeaderboard();
    }

    // ---------------------------------------------------------------
    //  LOAD FLOW: pastikan sign-in -> ambil top N -> populate UI
    // ---------------------------------------------------------------
    private async void LoadLeaderboard()
    {
        if (isLoading) return;
        isLoading = true;

        ShowStatus("Loading leaderboard...");

        bool signedIn = await EnsureSignedInAsync();
        if (!signedIn)
        {
            ShowStatus("Could not sign in.");
            isLoading = false;
            return;
        }

        LeaderboardPageResult result = await LeaderboardManager.Instance.GetTopScoresAsync(topEntriesCount, 0);

        PopulateList(result.Entries);

        isLoading = false;
    }

    private async Task<bool> EnsureSignedInAsync()
    {
        if (AuthenticationManager.Instance == null)
        {
            LogError("AuthenticationManager not found.");
            return false;
        }

        await AuthenticationManager.Instance.WaitForInitializationAsync();

        if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
        {
            Log("Already signed in, skip anonymous login.");
            return true;
        }

        try
        {
            Log("Not signed in, logging in anonymously to view leaderboard...");
            await AuthenticationManager.Instance.LoginAnonimAsync(anonymousViewerName);
            return AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn;
        }
        catch (Exception ex)
        {
            LogError($"Anonymous login failed: {ex.Message}");
            return false;
        }
    }

    // ---------------------------------------------------------------
    //  POPULATE
    // ---------------------------------------------------------------
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

        ShowStatus(entries.Count == 0 ? "No scores yet." : string.Empty);
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

    private void ShowStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private void Log(string message)
    {
        if (logDebugMessages) Debug.Log($"[MainLeaderboardUI] {message}");
    }

    private void LogError(string message)
    {
        if (logDebugMessages) Debug.LogError($"[MainLeaderboardUI] {message}");
    }
}