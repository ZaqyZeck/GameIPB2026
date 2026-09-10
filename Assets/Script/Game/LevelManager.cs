using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LevelManager : MonoBehaviour
{
    public enum GameState { Playing, Pause, End}

    public GameState state = GameState.Playing;
    public bool IsPlaying => state == GameState.Playing;
    public bool IsEnded => state == GameState.End;
    public bool IsPause => state == GameState.Pause;

    private void OnEnable()
    {
        GameEventBus.OnLevelWin += OnLevelWin;
        GameEventBus.OnPause += OnLevelPause;
        GameEventBus.OnResume += OnLevelResume;
    }

    private void OnDisable()
    {
        GameEventBus.OnLevelWin -= OnLevelWin;
        GameEventBus.OnPause -= OnLevelPause;
        GameEventBus.OnResume -= OnLevelResume;
    }

    public void ChangeState(GameState state)
    {
        switch(state)
        {
            case GameState.Playing:
                break;
            case GameState.Pause:
                break;
            case GameState.End:
                break;
        }
    }

    private async void OnLevelWin()
    {
        float reputationScore = ReputationManager.Instance.GetScore();

        // submint score ke leaderboard secara anonim dengan nama "ben"
        await LeaderboardManager.Instance.SubmitAnonymousScoreAsync((long) reputationScore, "anonim");
        ChangeState(GameState.End);
    } 

    private void OnLevelPause()
    {
        ChangeState(GameState.Pause);
    }

    private void OnLevelResume()
    {
        ChangeState(GameState.Playing);
    }
}
