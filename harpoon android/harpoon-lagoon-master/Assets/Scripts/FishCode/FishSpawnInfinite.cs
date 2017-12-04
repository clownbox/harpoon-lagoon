using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Advertisements;
[Serializable]
public class FishTypeAndBaseAndMult
{
	public FishSpawnInfinite.FishSpecies fishType;
	public int howMany;
	// public FishMoverBasic.FishMove moveStyle;
}

[Serializable]
public class FishKindWithinLevel
{
	public List<FishTypeAndBaseAndMult> fishKinds;
	public float weatherTarget = 0.0f;
	public bool hasTurtle = true;
	public bool hasOctopus = true;
	public int bronze;
	public int silver;
	public int gold;
	public ScoreManager.Medal medalEarned;
}

[Serializable]
public class FishLevelSeq
{
	public List<FishKindWithinLevel> fishLevelSeq;
}

public class FishSpawnInfinite : MonoBehaviour {
	public enum FishSpecies
	{
		STANDARD,
		SHIFTY,
		TINY,
		GOLD,
		BLOWFISH,
		SWORDFISH
	};
	public GameObject showEndScreen;
	public Text endScreenText;
	public MedalSwitch[] endMedals;

	public Text scoreSummaryText;

	public GameObject[] basicTypes;
	public List<FishLevelSeq> fishLevelOption;
	public int whichFishSeq = 0;
	public static FishSpawnInfinite instance;
	public int levelNow = 0;
	public TextFadeOut showDayText;
	public TextFadeOut showMedalText;
	public WeatherController weatherMaster;

	public bool spreadsheetDataLoaded = false;

	public runLaps turtle;
	public runLaps octopus;

	private int totalFishTillRespawn = 0;

	List<GameObject> fishList;

    //Rewarded Video Ads
    public int check = 1;
    public GameObject TripOver;
    public GameObject GameGUI;
	/*public void Restart() { // if wanting to use this again need to revisit how it gets called instead of FishKilledAndOffScreen_Refill
		foreach(GameObject GOFish in fishList) {
			if(GOFish) {
				Destroy(GOFish);
			}
		}
		levelNow = 0;
		totalFishTillRespawn = 0;
		SpawnForLevel();
	}*/

	public int BronzeGoal() {
		if(fishLevelOption.Count <= whichFishSeq || 
			fishLevelOption[whichFishSeq].fishLevelSeq.Count <= levelNow) {
			return 0;
		}

		if(spreadsheetDataLoaded) {
			return fishLevelOption[whichFishSeq].fishLevelSeq[levelNow].bronze;
		} else {
			return 0;
		}
	}
	public int SilverGoal() {
		if(fishLevelOption.Count <= whichFishSeq || 
			fishLevelOption[whichFishSeq].fishLevelSeq.Count <= levelNow) {
			return 0;
		}

		if(spreadsheetDataLoaded) {
			return fishLevelOption[whichFishSeq].fishLevelSeq[levelNow].silver;
		} else {
			return 0;
		}
	}
	public int GoldGoal() {
		if(fishLevelOption.Count <= whichFishSeq || 
			fishLevelOption[whichFishSeq].fishLevelSeq.Count <= levelNow) {
			return 0;
		}

		if(spreadsheetDataLoaded) {
			return fishLevelOption[whichFishSeq].fishLevelSeq[levelNow].gold;
		} else {
			return 0;
		}
	}

	public void ResetDay() {
		levelNow = 0;
		SpawnForLevel();
		Debug.Log("ResetDay");
	}

	bool blockLevelAdvance = false;

   
	IEnumerator DelayBetweenStages() {
		// Debug.Log("DelayBetweenStages started " + Time.time);
        TripOver.SetActive(false);
       // blockLevelAdvance = false;
       
        DashedLine.enableHoldLine = true;
        MenuStateMachine.instance.AllMenusOffExcept(GameGUI);
        
		ScoreManager.Medal medalWon = ScoreManager.instance.scoreMedalMeasure();

		fishLevelOption[whichFishSeq].fishLevelSeq[levelNow].medalEarned = medalWon;
		if(medalWon != ScoreManager.Medal.Fail) {
			showMedalText.showMedalMessage("" + medalWon);

			yield return new WaitForSeconds(Input.GetKey(KeyCode.A) == false &&
				Input.GetKey(KeyCode.B) == false ? 2.5f : 0.1f);
		}

		if(medalWon == ScoreManager.Medal.Fail) {
            if (check == 1)
            {
                ScoreManager.instance.ForceGameOver();
            }
            else if(check==2)
            {
               // MenuStateMachine.instance.AllMenusOffExcept(TripOver);
               // yield break;
                blockLevelAdvance = false;
            }
           //MenuStateMachine.instance.AllMenusOffExcept(TripOver);
          ////  Time.timeScale = 0;
          //  while (check == 0)
          //  {
          //      Debug.Log("Check=0");

                
          //  }
          //  if (check == 1)
          //  {
          //      ScoreManager.instance.ForceGameOver();
          //      Debug.Log("Check=1");
          //      yield break;
          //  }
          //  else if (check == 2)
          //  {
          //      Debug.Log("Check=2");
          //      blockLevelAdvance = false;
          //  }


        }
        

		if(blockLevelAdvance == false) { 
			//levelNow++; 
		} else {
			blockLevelAdvance = false;
		}

		if(levelNow >= fishLevelOption[whichFishSeq].fishLevelSeq.Count) {
			showEndScreen.SetActive(true);
			string medalSummary = "";

			for(int i = 0; i < endMedals.Length; i++) {
				endMedals[i].SetMedal(ScoreManager.Medal.Fail);
			}
			int bronzeFound = 0, silverFound = 0, goldFound = 0; 
			for(int i=0;i<fishLevelOption[whichFishSeq].fishLevelSeq.Count; i++) {
				medalSummary +=
					(i+1)+". "+fishLevelOption[whichFishSeq].fishLevelSeq[i].medalEarned;

				switch(fishLevelOption[whichFishSeq].fishLevelSeq[i].medalEarned) {
				case ScoreManager.Medal.Bronze:
					bronzeFound++;
					break;
				case ScoreManager.Medal.Silver:
					silverFound++;
					break;
				case ScoreManager.Medal.Gold:
					goldFound++;
					break;
				}

				if(i < fishLevelOption[whichFishSeq].fishLevelSeq.Count - 1) {
					medalSummary += "\n";
				}

				endMedals[i].SetMedal(
					fishLevelOption[whichFishSeq].fishLevelSeq[i].medalEarned
				);
			}

			int perBronze = 100;
			int perSilver = 250;
			int perGold = 500;
			int allBronze = perBronze * bronzeFound;
			int allSilver = perSilver * silverFound;
			int allGold = perGold * goldFound;
			int allTotal = allBronze+allSilver+allGold;
			int totalDays = bronzeFound+silverFound+goldFound;

			int bestYet = PlayerPrefs.GetInt("BestScore"+totalDays, 0);
			string endText = " points";
			if(bestYet < allTotal) {
				bestYet = allTotal;
				endText = " (new!)";
				PlayerPrefs.SetInt("BestScore" + totalDays, bestYet);
			} else if(bestYet == allTotal) {
				endText = " (tie!)";
			}

			scoreSummaryText.text = "Bronze "+perBronze+" x "+bronzeFound+" = "+allBronze+" points\n"+
				"Silver "+perSilver+" x "+silverFound+" = "+allSilver+" points\n"+
				"Gold "+perGold+" x "+goldFound+" = "+allGold+" points\n"+
				"Your Trip Total: "+allTotal+" points\n"+
				"Your "+totalDays+"-Day Best: "+bestYet+endText;

			AnalyticsResult arReturn = Analytics.CustomEvent("gameWon", new Dictionary<string, object>
				{
					{ "daysPlayed", totalDays },
					{ "bronze", bronzeFound },
					{ "silver", silverFound },
					{ "gold", goldFound },
					{ "tripScore", allTotal }
				});
			Debug.Log("gameWon analytics worked: " + arReturn);

			endScreenText.text = "";//medalSummary;
			yield break;
		}

		switch(levelNow) {
		case 10:
			MenuStateMachine.instance.DidAchivement(MenuStateMachine.ACHIEVEMENT_ENUM.dayTen,100.0f);
			break;
		case 20:
			MenuStateMachine.instance.DidAchivement(MenuStateMachine.ACHIEVEMENT_ENUM.dayTwenty,100.0f);
			break;
		case 30:
			MenuStateMachine.instance.DidAchivement(MenuStateMachine.ACHIEVEMENT_ENUM.dayThirty,100.0f);
			break;
		}
		ScoreManager.instance.ResetScore(false);
		SpawnForLevel();
	}
    IEnumerator MyDelayBetweenStages()
    {
        // Debug.Log("DelayBetweenStages started " + Time.time);

        ScoreManager.Medal medalWon = ScoreManager.instance.scoreMedalMeasure();

        fishLevelOption[whichFishSeq].fishLevelSeq[levelNow].medalEarned = medalWon;
        if (medalWon != ScoreManager.Medal.Fail)
        {
            showMedalText.showMedalMessage("" + medalWon);

            yield return new WaitForSeconds(Input.GetKey(KeyCode.A) == false &&
                Input.GetKey(KeyCode.B) == false ? 2.5f : 0.1f);
        }

        if (medalWon == ScoreManager.Medal.Fail)
        {
            yield return new WaitForSeconds(1);
            MenuStateMachine.instance.AllMenusOffExcept(TripOver);
            DashedLine.enableHoldLine = false;
          //  blockLevelAdvance =true;
            
           levelNow--;
            
        }
        if (blockLevelAdvance == false)
        {
            levelNow++;
        }
        else
        {
            blockLevelAdvance = false;
        }

        if (levelNow >= fishLevelOption[whichFishSeq].fishLevelSeq.Count)
        {
            showEndScreen.SetActive(true);
            string medalSummary = "";

            for (int i = 0; i < endMedals.Length; i++)
            {
                endMedals[i].SetMedal(ScoreManager.Medal.Fail);
            }
            int bronzeFound = 0, silverFound = 0, goldFound = 0;
            for (int i = 0; i < fishLevelOption[whichFishSeq].fishLevelSeq.Count; i++)
            {
                medalSummary +=
                    (i + 1) + ". " + fishLevelOption[whichFishSeq].fishLevelSeq[i].medalEarned;

                switch (fishLevelOption[whichFishSeq].fishLevelSeq[i].medalEarned)
                {
                    case ScoreManager.Medal.Bronze:
                        bronzeFound++;
                        break;
                    case ScoreManager.Medal.Silver:
                        silverFound++;
                        break;
                    case ScoreManager.Medal.Gold:
                        goldFound++;
                        break;
                }

                if (i < fishLevelOption[whichFishSeq].fishLevelSeq.Count - 1)
                {
                    medalSummary += "\n";
                }

                endMedals[i].SetMedal(
                    fishLevelOption[whichFishSeq].fishLevelSeq[i].medalEarned
                );
            }

            int perBronze = 100;
            int perSilver = 250;
            int perGold = 500;
            int allBronze = perBronze * bronzeFound;
            int allSilver = perSilver * silverFound;
            int allGold = perGold * goldFound;
            int allTotal = allBronze + allSilver + allGold;
            int totalDays = bronzeFound + silverFound + goldFound;

            int bestYet = PlayerPrefs.GetInt("BestScore" + totalDays, 0);
            string endText = " points";
            if (bestYet < allTotal)
            {
                bestYet = allTotal;
                endText = " (new!)";
                PlayerPrefs.SetInt("BestScore" + totalDays, bestYet);
            }
            else if (bestYet == allTotal)
            {
                endText = " (tie!)";
            }

            scoreSummaryText.text = "Bronze " + perBronze + " x " + bronzeFound + " = " + allBronze + " points\n" +
                "Silver " + perSilver + " x " + silverFound + " = " + allSilver + " points\n" +
                "Gold " + perGold + " x " + goldFound + " = " + allGold + " points\n" +
                "Your Trip Total: " + allTotal + " points\n" +
                "Your " + totalDays + "-Day Best: " + bestYet + endText;

            AnalyticsResult arReturn = Analytics.CustomEvent("gameWon", new Dictionary<string, object>
				{
					{ "daysPlayed", totalDays },
					{ "bronze", bronzeFound },
					{ "silver", silverFound },
					{ "gold", goldFound },
					{ "tripScore", allTotal }
				});
            Debug.Log("gameWon analytics worked: " + arReturn);

            endScreenText.text = "";//medalSummary;
            yield break;
        }

        switch (levelNow)
        {
            case 10:
                MenuStateMachine.instance.DidAchivement(MenuStateMachine.ACHIEVEMENT_ENUM.dayTen, 100.0f);
                break;
            case 20:
                MenuStateMachine.instance.DidAchivement(MenuStateMachine.ACHIEVEMENT_ENUM.dayTwenty, 100.0f);
                break;
            case 30:
                MenuStateMachine.instance.DidAchivement(MenuStateMachine.ACHIEVEMENT_ENUM.dayThirty, 100.0f);
                break;
        }
        ScoreManager.instance.ResetScore(false);
        SpawnForLevel();

       
    }

    public void watchAdClick()
    {
        Debug.Log("Level Now : " + levelNow);
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
                check = 2;
                StartCoroutine(DelayBetweenStages());
                Debug.Log("Finished");
                break;
            case ShowResult.Skipped:
                check = 1;
                Debug.Log("Skipped");
                StartCoroutine(DelayBetweenStages());
                break;
            case ShowResult.Failed:
                check =1;
                StartCoroutine(DelayBetweenStages());
                Debug.Log("Fail");
                break;
        }
       // Time.timeScale = 1;
        
    }
    //public void onGameOver()
    //{
    //    ScoreManager.Medal medalWon = ScoreManager.instance.scoreMedalMeasure();

    //    fishLevelOption[whichFishSeq].fishLevelSeq[levelNow].medalEarned = medalWon;
    //    if (medalWon != ScoreManager.Medal.Fail)
    //    {
    //        showMedalText.showMedalMessage("" + medalWon);

    //        yield return new WaitForSeconds(Input.GetKey(KeyCode.A) == false &&
    //            Input.GetKey(KeyCode.B) == false ? 2.5f : 0.1f);
    //    }

    //    if (medalWon == ScoreManager.Medal.Fail)
    //    {
    //        MenuStateMachine.instance.AllMenusOffExcept(TripOver);
    //        Time.timeScale = 0;
    //        if (check == true)
    //        {
    //            Debug.Log("Check=true");

    //            blockLevelAdvance = false;
    //        }
    //        if (check == false)
    //        {
    //            ScoreManager.instance.ForceGameOver();
    //            Debug.Log("Check=false");
    //          //  yield break;
    //        }


    //    }
    //}
    public void NextLevel()
    {
        if (MenuStateMachine.instance.notInTut())
        {
            StartCoroutine(MyDelayBetweenStages());
        }
        else
        {
            //if(MenuStateMachine.instance.tutStep < MenuStateMachine.TUTORIAL_PHASE.Blowfish) {
            levelNow = 0;
            /*} else {
                levelNow = 2;
            }*/
            ScoreManager.instance.ResetScore(false);
            SpawnForLevel();
        }
    }

	public void FishKilledAndOffScreen(GameObject whichFish) {
		fishList.Remove(whichFish);

		totalFishTillRespawn--;

		if(ScoreManager.instance.ShowGameOverIfNeeded() == false) {
			if(totalFishTillRespawn <= 0) {
				NextLevel();
			}
		}
	}

	public void RemoveAll() {
		foreach(GameObject eachFish in fishList) {
			Destroy(eachFish);
		}
		fishList = new List<GameObject>();
	}

	public void UpdateText() {
		showDayText.showDay(levelNow);
		if(levelNow >= 1) {
			FMODUnity.RuntimeManager.PlayOneShot("event:/round_end");
		}
	}

	public void AddOneFish(int fishKind = 2) {
		GameObject GOFish = (GameObject)GameObject.Instantiate(basicTypes[fishKind]);
		FishMoverBasic fmbScript = GOFish.GetComponent<FishMoverBasic>();
		GOFish.transform.position =
			SeaBounds.instance.randPosBandBias(fmbScript.depthBiasOdds,
				fmbScript.shallowPerc,
				fmbScript.deepPerc);
		fishList.Add(GOFish);
	}

	public void SpawnForLevel() {
		SharkHurry.instance.retreating = true;
		UpdateText();
		RemoveAll();
		totalFishTillRespawn = 0;
		int levCapped = levelNow;
		if(levCapped >= fishLevelOption[whichFishSeq].fishLevelSeq.Count) {
			levCapped = fishLevelOption[whichFishSeq].fishLevelSeq.Count - 1;
			Debug.Log("LEVEL DEFINITION MISSING FOR seq " + whichFishSeq + " on levelNow: " + levelNow);
			ScoreManager.instance.InstantGameOver();
		}
		for(int i=0;i<fishLevelOption[whichFishSeq].fishLevelSeq[levCapped].fishKinds.Count;i++) {
			int howMany = fishLevelOption[whichFishSeq].fishLevelSeq[levCapped].fishKinds[i].howMany;
			for(int ii=0;ii<howMany;ii++) {
				FishSpecies whichKind = fishLevelOption[whichFishSeq].fishLevelSeq[levCapped].fishKinds[i].fishType;
				GameObject whichPrefab = basicTypes[(int)(whichKind)];
				GameObject GOFish = (GameObject)GameObject.Instantiate(whichPrefab);
				FishMoverBasic fmbScript = GOFish.GetComponent<FishMoverBasic>();
				GOFish.transform.position =
					SeaBounds.instance.randPosBandBias(fmbScript.depthBiasOdds,
					                                   fmbScript.shallowPerc,
					                                   fmbScript.deepPerc);
				GOFish.name = "Fish"+ whichPrefab.name +" " + (ii+1);
				// fmbScript.aiMode = fishLevelOption[whichFishSeq].fishLevelSeq[levCapped].fishKinds[i].moveStyle;
				switch(whichKind) {
				case FishSpecies.STANDARD:
				case FishSpecies.BLOWFISH:
					if(UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f) {
						fmbScript.aiMode = FishMoverBasic.FishMove.CIRCLE_CW;
					} else {
						fmbScript.aiMode = FishMoverBasic.FishMove.CIRCLE_CCW;
					}
					break;
				case FishSpecies.TINY:
					fmbScript.aiMode = FishMoverBasic.FishMove.VERTICAL_LINE;
					break;
				case FishSpecies.SHIFTY:
					fmbScript.aiMode = FishMoverBasic.FishMove.HORIZONTAL_LINE;
					break;
				case FishSpecies.GOLD:
					fmbScript.aiMode = FishMoverBasic.FishMove.STANDARD_SPREAD;
					break;
				case FishSpecies.SWORDFISH:
					fmbScript.aiMode = FishMoverBasic.FishMove.SWORDFISH_HOP;
					break;
				}
				fishList.Add(GOFish);
				totalFishTillRespawn++;
			}
		}
		if(fishLevelOption[whichFishSeq].fishLevelSeq[levCapped].hasTurtle) {
			if(turtle.gameObject.activeSelf == false) {
				turtle.Restart();
			}
			turtle.gameObject.SetActive(true);
		} else {
			turtle.gameObject.SetActive(false);
		}
		if(fishLevelOption[whichFishSeq].fishLevelSeq[levCapped].hasOctopus) {
			if(octopus.gameObject.activeSelf == false) {
				octopus.Restart();
			}
			octopus.gameObject.SetActive(true);
		} else {
			octopus.gameObject.SetActive(false);
		}

		weatherMaster.NewWeatherValue( fishLevelOption[whichFishSeq].fishLevelSeq[levCapped].weatherTarget );
	}

	void CalcLevScoreGoals(int wasLevNum, out int levNum, out int minScore,
		FishKindWithinLevel currentSeq) {
		levNum = wasLevNum+1;
		minScore = 0;
		Debug.Log("Setting up score goals for level " + levNum);

		int totalFish = 0;
		int totalFishWithoutBlowfish = 0;

		int [] fishTally = new int[Enum.GetNames(typeof(FishSpecies)).Length];
		int fishTalliedComboTemp = 0;

		for(int i = 0; i < fishTally.Length; i++) {
			fishTally[i] = 0;
		}

		for(int ii = 0; ii < currentSeq.fishKinds.Count; ii++) {
			FishMoverBasic fmbScript =
				basicTypes[(int)(currentSeq.fishKinds[ii].fishType)].GetComponent<FishMoverBasic>();

			/* Debug.Log(fmbScript.scoreValue + " pt X " +
				currentSeq.fishKinds[ii].howMany); */
			totalFish += currentSeq.fishKinds[ii].howMany;
			if(currentSeq.fishKinds[ii].fishType != (FishSpecies)FishMoverBasic.FishBreed.BLOWFISH) {
				totalFishWithoutBlowfish += currentSeq.fishKinds[ii].howMany;
			}
			minScore += fmbScript.scoreValue *
				currentSeq.fishKinds[ii].howMany;
			fishTally[(int)currentSeq.fishKinds[ii].fishType] += currentSeq.fishKinds[ii].howMany;
		}
		fishTalliedComboTemp = totalFishWithoutBlowfish; // WAS TOTALFISH, does it need blowfish? for pair calc.

		/*for(int i=0;i<fishTally.Length;i++) {
			if(fishTally[i] > 5) {
				Debug.LogWarning("MIX FISH: MORE THAN 5 OF A KIND IN LEV: "+levNum);
			}
		}
		if(fishTalliedComboTemp < 3) {
			Debug.LogWarning("ADD FISH: UNDER 3 FISH IN LEV: \"+levNum);
		}
		if(fishTalliedComboTemp > 8) {
			Debug.LogWarning("REDUCE FISH: OVER 8 IN LEV: "+levNum);
		}*/

		int seriesCombos = 0;
		bool anyFoundSoTryAgain = true;
		bool [] whichCounted = new bool[fishTally.Length];
		int breedsFound = 0;
		while(anyFoundSoTryAgain) {
			anyFoundSoTryAgain = false;

			breedsFound = 0;
			for(var i = 0; i < whichCounted.Length; i++) {
				whichCounted[i] = false;
			}

			for(var eachFish = 0; eachFish < fishTally.Length; eachFish++) {
				if(fishTally[eachFish] > 0) {
					breedsFound++;
					whichCounted[eachFish] = true;
				}
				if(breedsFound>=3) {
					break;
				}
			}
				
			if(breedsFound>=3) {
				for(var eachFish = 0; eachFish < whichCounted.Length; eachFish++) {
					if(whichCounted[eachFish]) {
						fishTally[eachFish]--;
					}
				}
				seriesCombos++;
				fishTalliedComboTemp -= 3;
				anyFoundSoTryAgain = true;
			}
			/*
			if(fishTally[0] > 0 && fishTally[2] > 0 && fishTally[3] > 0) { // without 1
				fishTally[0]--;
				fishTally[2]--;
				fishTally[3]--;
				seriesCombos++;
				fishTalliedComboTemp -= 3;
				anyFoundSoTryAgain = true;
			}*/
		}

		int tripleCombos = 0;
		anyFoundSoTryAgain = true;
		while(anyFoundSoTryAgain) {
			anyFoundSoTryAgain = false;
			for(var eachBreed = 0; eachBreed < fishTally.Length; eachBreed++) {
				if(fishTally[eachBreed] >= 3 &&
					eachBreed != (int)FishMoverBasic.FishBreed.BLOWFISH) { // cannot triple hit
					fishTally[eachBreed] -= 3;
					tripleCombos++;
					fishTalliedComboTemp -= 3;
					anyFoundSoTryAgain = true;
				}
			}
				/*
			if(fishTally[1] >= 3) {
				fishTally[1]-=3;
				tripleCombos++;
				fishTalliedComboTemp -= 3;
				anyFoundSoTryAgain = true;
			}*/
		}

		int setsOfThreeEveryThrow = (int)(totalFishWithoutBlowfish / 3.0f); // was totalFish
		int fishLeftOver = totalFishWithoutBlowfish-setsOfThreeEveryThrow*3;
		int setsPoints = setsOfThreeEveryThrow * (ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE+
			ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE_THIRD);

		var lastPairFound = false;
		for(var i = 0; i < fishTally.Length; i++) {
			if(i != (int) FishMoverBasic.FishBreed.BLOWFISH) {
				lastPairFound = (lastPairFound || fishTally[i] >= 2);
			}
		}
		/*
			(fishTally[0] >= 2 || fishTally[1] >= 2 ||
				fishTally[2] >= 2 || fishTally[3] >= 2 ||
				fishTally[5] >= 2); */

		if(fishTalliedComboTemp == 2) {
			if(lastPairFound) {
				setsPoints += ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE_PAIR;
			} else {
				setsPoints += ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE;
			}
		}

		int seriesPoints = seriesCombos * ScoreManager.SCORE_PER_SERIES;
		int triplePoints = tripleCombos * ScoreManager.SCORE_PER_TRIPLE;
		/*Debug.Log("Series Combos: " + seriesCombos);
		Debug.Log("Series Score: " + seriesPoints);
		Debug.Log("Triple Combos: " + tripleCombos);
		Debug.Log("Triple Score: " + triplePoints);
		Debug.Log("Min Score: " + minScore);
		Debug.Log("Possible Sets Points: " + setsPoints);*/

		int maxPossibleScoreAboveMin = setsPoints +
			seriesPoints +
			triplePoints;

		int midPoints;

		midPoints = maxPossibleScoreAboveMin;
		if(seriesCombos > 0) {
			midPoints -= ScoreManager.SCORE_PER_SERIES;
		} else if(tripleCombos > 0) {
			midPoints -= ScoreManager.SCORE_PER_TRIPLE;
		} else if(lastPairFound) {
			var anyNonPair = false;
			for(var i = 0; i < fishTally.Length; i++) {
				if(i != (int) FishMoverBasic.FishBreed.BLOWFISH) {
					anyNonPair = (lastPairFound || fishTally[i] == 1);
				}
			}
			/*(fishTally[0] == 1 || fishTally[1] == 1 ||
				fishTally[2] == 1 || fishTally[3] == 1);
			*/
			if(anyNonPair && lastPairFound) {
				midPoints -= (ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE_PAIR -
								ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE);
			} else {
				midPoints -= ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE_PAIR;
			}
		} else {
			midPoints -= ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE_THIRD;
		}

		currentSeq.bronze = minScore+ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE;
		currentSeq.silver = minScore+midPoints;

		if(currentSeq.bronze > currentSeq.silver) {
			int swapVal = currentSeq.bronze;
			currentSeq.bronze = currentSeq.silver;
			currentSeq.silver = swapVal;
		}
		currentSeq.gold = minScore+maxPossibleScoreAboveMin;

		if(currentSeq.bronze == currentSeq.silver) {
			currentSeq.silver = (int)( (currentSeq.bronze+currentSeq.gold)/2 );
		}
	}

	void LoadLevelData() {
		int minScore = 0;

		if (spreadsheetDataLoaded){
			Debug.Log ("spreadsheet has already been loaded, keeping level data in memory");
			return;
		}

		spreadsheetDataLoaded = true;

		Debug.Log ("spreadsheet level data loading from file...");

		TextAsset unitData = Resources.Load ("LevelSets") as TextAsset;
		string [] unitRows = unitData.text.Split (new char[]{'\r'});

		fishLevelOption = new List<FishLevelSeq>();

		FishLevelSeq nextLev = new FishLevelSeq();
		nextLev.fishLevelSeq = new List<FishKindWithinLevel>();

		FishKindWithinLevel currentSeq = null;
		FishTypeAndBaseAndMult tempFish;
		int levNum = 0;
		int levSet = 1;
		for (int i=1; i < unitRows.Length; i++){//i=1 to skip column headers
			string [] unitCols = unitRows[i].Split (',');

			switch(unitCols[0]) {
			case "NEXT_LEVEL_SET":
				levNum = 0;
				Debug.Log("DIFFERENT LEVEL SET " + levSet);
				levSet++;
				CalcLevScoreGoals(levNum, out levNum, out minScore, currentSeq);

				nextLev.fishLevelSeq.Add(currentSeq);
				fishLevelOption.Add(nextLev);
				nextLev = new FishLevelSeq();
				nextLev.fishLevelSeq = new List<FishKindWithinLevel>();
				currentSeq = null;
				break;
			case "levSettings":
				if(currentSeq != null) {
					CalcLevScoreGoals(levNum, out levNum, out minScore, currentSeq);
					nextLev.fishLevelSeq.Add(currentSeq);
				}
				currentSeq = new FishKindWithinLevel();
				currentSeq.fishKinds = new List<FishTypeAndBaseAndMult>();
				currentSeq.hasTurtle = bool.Parse(unitCols[1]);
				currentSeq.hasOctopus = bool.Parse(unitCols[2]);
				currentSeq.weatherTarget = float.Parse(unitCols[3]);
				//currentSeq.bronze = int.Parse(unitCols[4]);
				//currentSeq.silver = int.Parse(unitCols[5]);
				//currentSeq.gold = int.Parse(unitCols[6]);
				break;
			case "addFish":
				tempFish = new FishTypeAndBaseAndMult();
				tempFish.fishType = (FishSpawnInfinite.FishSpecies)Enum.Parse(
					typeof(FishSpawnInfinite.FishSpecies), unitCols[1] );
				tempFish.howMany = int.Parse(unitCols[2]);
				/* tempFish.moveStyle = (FishMoverBasic.FishMove)Enum.Parse(
					typeof(FishMoverBasic.FishMove), unitCols[3] );*/
				currentSeq.fishKinds.Add(tempFish);
				break;
			default:
				Debug.Log("UNPARSED LINE: " + unitRows[0]);
				break;
			}

			/* stringToUnitType(unitCols[0]), 
				unitCols[1],
				bool.Parse(unitCols[11]), 
				int.Parse(unitCols[12]), */

			// fishLevelOption.Add (nextCard);
		}

		Debug.Log("=== Final level tally");
		CalcLevScoreGoals(levNum, out levNum, out minScore, currentSeq);

		if(currentSeq != null) {
			nextLev.fishLevelSeq.Add(currentSeq);
		}
		fishLevelOption.Add(nextLev);
	}

	void Awake() {
		instance = this;
        Advertisement.Initialize("1562108", true);
	}

	// Use this for initialization
	void Start () {
		fishList = new List<GameObject>();
		GameObject weatherObj = GameObject.Find("Environment");
		weatherMaster = weatherObj.GetComponent<WeatherController>();

		LoadLevelData();

		SpawnForLevel();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.C)) {
			FishMoverBasic.fishHalted = !FishMoverBasic.fishHalted;
		}

		if(Input.GetKeyDown(KeyCode.B)) {
			blockLevelAdvance = true;
			NextLevel();
		}

		if(Input.GetKeyDown(KeyCode.A)) {
			Debug.Log("day set (whichFishSeq): "+whichFishSeq);
			Debug.Log("also uncomment score cheat at top of scoreMedalMeasure() in ScoreManager.cs");
			NextLevel();
		}
	}
}
