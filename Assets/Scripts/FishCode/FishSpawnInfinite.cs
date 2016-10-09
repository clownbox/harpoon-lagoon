﻿using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

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
}

[Serializable]
public class FishLevelSeq
{
	public List<FishKindWithinLevel> fishLevelSeq;
}

public class FishSpawnInfinite : MonoBehaviour {
	public enum FishSpecies
	{
		SHIFTY,
		TINY,
		STANDARD,
		GOLD
	};
	public GameObject[] basicTypes;
	public List<FishLevelSeq> fishLevelOption;
	public int whichFishSeq = 0;
	public static FishSpawnInfinite instance;
	private int levelNow = 0;
	public TextFadeOut showDayText;
	public WeatherController weatherMaster;

	public bool spreadsheetDataLoaded = false;

	public runLaps turtle;
	public runLaps octopus;

	private int totalFishTillRespawn = 0;

	List<GameObject> fishList;

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
		if(spreadsheetDataLoaded) {
			return fishLevelOption[whichFishSeq].fishLevelSeq[levelNow].bronze;
		} else {
			return 0;
		}
	}
	public int SilverGoal() {
		if(spreadsheetDataLoaded) {
			return fishLevelOption[whichFishSeq].fishLevelSeq[levelNow].silver;
		} else {
			return 0;
		}
	}
	public int GoldGoal() {
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

	IEnumerator DelayBetweenStages() {
		Debug.Log("DelayBetweenStages started " + Time.time);

		ScoreManager.Medal medalWon = ScoreManager.instance.scoreMedalMeasure();

		showDayText.showMedalMessage("" + medalWon);

		yield return new WaitForSeconds(2.5f);

		if(medalWon == ScoreManager.Medal.Fail) {
			ScoreManager.instance.ForceGameOver();
			yield break;
		}

		levelNow++;

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

	public void NextLevel() {
		if(MenuStateMachine.instance.notInTut()) {
			StartCoroutine(DelayBetweenStages());
		} else {
			if(MenuStateMachine.instance.tutStep < MenuStateMachine.TUTORIAL_PHASE.ExtraSpear) {
				levelNow = 0;
			} else {
				levelNow = 2;
			}
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
				fishList.Add(GOFish);
				// fmbScript.aiMode = fishLevelOption[whichFishSeq].fishLevelSeq[levCapped].fishKinds[i].moveStyle;
				switch(whichKind) {
				case FishSpecies.STANDARD:
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
				}
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
		Debug.Log("Min score tally for level " + levNum);

		int totalFish = 0;

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
			minScore += fmbScript.scoreValue *
				currentSeq.fishKinds[ii].howMany;
			fishTally[(int)currentSeq.fishKinds[ii].fishType] += currentSeq.fishKinds[ii].howMany;
		}
		fishTalliedComboTemp = totalFish;

		int seriesCombos = 0;
		bool anyFoundSoTryAgain = true;
		while(anyFoundSoTryAgain) {
			anyFoundSoTryAgain = false;
			if(fishTally[0] > 0 && fishTally[1] > 0 && fishTally[2] > 0) { // without 3
				fishTally[0]--;
				fishTally[1]--;
				fishTally[2]--;
				seriesCombos++;
				fishTalliedComboTemp -= 3;
				anyFoundSoTryAgain = true;
			}
			if(fishTally[0] > 0 && fishTally[1] > 0 && fishTally[3] > 0) { // without 2
				fishTally[0]--;
				fishTally[1]--;
				fishTally[3]--;
				seriesCombos++;
				fishTalliedComboTemp -= 3;
				anyFoundSoTryAgain = true;
			}
			if(fishTally[0] > 0 && fishTally[2] > 0 && fishTally[3] > 0) { // without 1
				fishTally[0]--;
				fishTally[2]--;
				fishTally[3]--;
				seriesCombos++;
				fishTalliedComboTemp -= 3;
				anyFoundSoTryAgain = true;
			}
			if(fishTally[1] > 0 && fishTally[2] > 0 && fishTally[3] > 0) { // without 0
				fishTally[1]--;
				fishTally[2]--;
				fishTally[3]--;
				seriesCombos++;
				fishTalliedComboTemp -= 3;
				anyFoundSoTryAgain = true;
			}
		}

		int tripleCombos = 0;
		anyFoundSoTryAgain = true;
		while(anyFoundSoTryAgain) {
			anyFoundSoTryAgain = false;
			if(fishTally[0] >= 3) {
				fishTally[0]-=3;
				tripleCombos++;
				fishTalliedComboTemp -= 3;
				anyFoundSoTryAgain = true;
			}
			if(fishTally[1] >= 3) {
				fishTally[1]-=3;
				tripleCombos++;
				fishTalliedComboTemp -= 3;
				anyFoundSoTryAgain = true;
			}
			if(fishTally[2] >= 3) {
				fishTally[2]-=3;
				tripleCombos++;
				fishTalliedComboTemp -= 3;
				anyFoundSoTryAgain = true;
			}
			if(fishTally[3] >= 3) {
				fishTally[3]-=3;
				tripleCombos++;
				fishTalliedComboTemp -= 3;
				anyFoundSoTryAgain = true;
			}
		}

		int setsOfThreeEveryThrow = (int)(totalFish / 3.0f);
		int fishLeftOver = totalFish-setsOfThreeEveryThrow*3;
		int setsPoints = setsOfThreeEveryThrow * (ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE+
			ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE_THIRD);

		var lastPairFound = 
			(fishTally[0] >= 2 || fishTally[1] >= 2 ||
				fishTally[2] >= 2 || fishTally[3] >= 2);

		if(fishTalliedComboTemp == 2) {
			if(lastPairFound) {
				setsPoints += ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE_PAIR;
			} else {
				setsPoints += ScoreManager.SCORE_PER_EXTRA_FISH_ON_POLE;
			}
		}

		int seriesPoints = seriesCombos * ScoreManager.SCORE_PER_SERIES;
		int triplePoints = tripleCombos * ScoreManager.SCORE_PER_TRIPLE;
		Debug.Log("Series Combos: " + seriesCombos);
		Debug.Log("Series Score: " + seriesPoints);
		Debug.Log("Triple Combos: " + tripleCombos);
		Debug.Log("Triple Score: " + triplePoints);
		Debug.Log("Min Score: " + minScore);
		Debug.Log("Possible Sets Points: " + setsPoints);

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
			var anyNonPair = (fishTally[0] == 1 || fishTally[1] == 1 ||
				fishTally[2] == 1 || fishTally[3] == 1);
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
		currentSeq.gold = minScore+maxPossibleScoreAboveMin;
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
		for (int i=1; i < unitRows.Length; i++){//i=1 to skip column headers
			string [] unitCols = unitRows[i].Split (',');

			switch(unitCols[0]) {
			case "NEXT_LEVEL_SET":
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
		if(Input.GetKeyDown(KeyCode.A)) {
			Debug.Log("whichFishSeq: "+whichFishSeq);
			NextLevel();
		}
	}
}
