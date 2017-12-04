using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
public class TripOver : MonoBehaviour {
    public GameObject GameGUI;
	// Use this for initialization
	void Start () {
        Time.timeScale = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    //My Code For Rewared Ad
    /// </summary>
    public void watchAdClick()
    {
        if (Advertisement.IsReady())
        {
            Advertisement.Show("rewardedVideo", new ShowOptions() { resultCallback = HandleAdResult });

        }
    }
    private void HandleAdResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                FishSpawnInfinite.instance.levelNow++;
                Debug.Log("Finished");
                break;
            case ShowResult.Skipped:
               // check = true;
                Debug.Log("Skipped");
               // FishSpawnInfinite.instance.levelNow++;
                break;
            case ShowResult.Failed:
              //  check = true;
                //FishSpawnInfinite.instance.levelNow++;
                Debug.Log("Fail");
                break;
        }
        Time.timeScale = 1;
        MenuStateMachine.instance.AllMenusOffExcept(GameGUI);
    }
}
