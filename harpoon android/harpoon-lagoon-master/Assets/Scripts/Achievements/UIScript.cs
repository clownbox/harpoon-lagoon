﻿
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{

    public static UIScript Instance { get; private set; }

    // Use this for initialization
    void Start()
    {
        Instance = this;
    }

    //[SerializeField]
    //private Text pointsTxt;

    //public void GetPoint()
    //{
    //    ManagerScript.Instance.IncrementCounter();
    //}

    //public void Restart()
    //{
    //    ManagerScript.Instance.RestartGame();
    //}

    //public void Increment()
    //{
    //    PlayGamesScript.IncrementAchievement(GPGSIds.achievement_3_fish_big_to_small, 5);
    //}

    public void Unlock()
    {
        PlayGamesScript.UnlockAchievement(GPGSIds.achievement_3_fish_big_to_small);
    }

    public void ShowAchievements()
    {
        PlayGamesScript.ShowAchievementsUI();
    }

    public void ShowLeaderboards()
    {
        PlayGamesScript.ShowLeaderboardsUI();
    }

    //public void UpdatePointsText()
    //{
    //    pointsTxt.text = ManagerScript.Counter.ToString();
    //}
}

