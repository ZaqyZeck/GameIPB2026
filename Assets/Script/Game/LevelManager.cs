using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public enum GameState { Playing, Pause, End}

    public GameState state = GameState.Playing;
    public bool IsPlaying => state == GameState.Playing;
    public bool IsEnded => state == GameState.End;
    public bool IsPause => state == GameState.Pause;

    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        GameEventBus.OnSubmitScore += OnSubmitScore;
        GameEventBus.OnWin += OnLevelWin;
        GameEventBus.OnPause += OnLevelPause;
        GameEventBus.OnResume += OnLevelResume;
    }

    private void OnDisable()
    {
        GameEventBus.OnSubmitScore -= OnSubmitScore;
        GameEventBus.OnWin -= OnLevelWin;
        GameEventBus.OnPause -= OnLevelPause;
        GameEventBus.OnResume -= OnLevelResume;
    }

    public void ChangeState(GameState state)
    {
        switch(state)
        {
            case GameState.Playing:
                //Time.timeScale = 1f;
                break;
            case GameState.Pause:
                //Time.timeScale = 0f;
                break;
            case GameState.End:
                //Time.timeScale = 0f;
                break;
        }
        this.state = state;
    }

    private void OnLevelWin()
    {
        //float reputationScore = ReputationManager.Instance.GetScore();

        // submint score ke leaderboard secara anonim dengan nama "ben"
        //await LeaderboardManager.Instance.SubmitAnonymousScoreAsync((long) reputationScore, "anonim");
        ChangeState(GameState.End);
    } 

    private void OnLevelPause()
    {
        ChangeState(GameState.Pause);
        Debug.LogWarning("terpause");
    }

    private void OnLevelResume()
    {
        ChangeState(GameState.Playing);
        Debug.LogWarning("resume");
    }

    private async void OnSubmitScore(string playerName)
    {
        float reputationScore = ReputationManager.Instance.GetScore();

        // submint score ke leaderboard secara anonim dengan nama "ben"
        Debug.LogWarning(playerName + " itu nama player yg disubmit di leaderboard");
        try
        {
            await LeaderboardManager.Instance.SubmitAnonymousScoreAsync((long) reputationScore, playerName);

        }
        catch
        {
            //Debug.LogError("submit")
        }
        

        UIGameplay.ResetReputation();

        GameManager.Instance.LoadMainMenu();
    }
}
