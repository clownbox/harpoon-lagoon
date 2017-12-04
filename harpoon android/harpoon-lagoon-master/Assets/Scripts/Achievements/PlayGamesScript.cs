
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class PlayGamesScript : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();

        SignIn();
    }

    void SignIn()
    {
        Debug.Log("Sign In");
        Social.localUser.Authenticate(success => { });
    }

    #region Achievements
    public static void UnlockAchievement(string id)
    {
        Social.ReportProgress(id, 40, success => { });
    }

    //public static void IncrementAchievement(string id, int stepsToIncrement)
    //{
    //    PlayGamesPlatform.Instance.IncrementAchievement(id, stepsToIncrement, success => { });
    //}

    public static void ShowAchievementsUI()
    {
        Debug.Log("Achievements Show");
        Social.ShowAchievementsUI();
    }
    #endregion /Achievements

    #region Leaderboards
    public static void AddScoreToLeaderboard(string leaderboardId, long score)
    {
        Social.ReportScore(score, leaderboardId, success => { });
    }

    public static void ShowLeaderboardsUI()
    {
        Social.ShowLeaderboardUI();
        ((PlayGamesPlatform)Social.Active).ShowLeaderboardUI();
    }
    #endregion /Leaderboards

}