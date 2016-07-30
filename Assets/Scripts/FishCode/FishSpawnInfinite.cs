﻿using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

[Serializable]
public class FishTypeAndBaseAndMult
{
	public FishSpawnInfinite.FishSpecies fishType;
	public int howMany;
	public FishMoverBasic.FishMove moveStyle;
}

[Serializable]
public class FishKindWithinLevel
{
	public FishTypeAndBaseAndMult[] fishKinds;
	public float weatherTarget = 0.0f;
	public bool hasTurtle = true;
	public bool hasOctopus = true;
}

[Serializable]
public class FishLevelSeq
{
	public FishKindWithinLevel[] fishLevelSeq;
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
	public FishLevelSeq[] fishLevelOption;
	public int whichFishSeq = 0;
	public static FishSpawnInfinite instance;
	private int levelNow = 0;
	public TextFadeOut showDayText;
	public WeatherController weatherMaster;

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

	public void ResetDay() {
		levelNow = 0;
		SpawnForLevel();
		Debug.Log("ResetDay");
	}

	public void NextLevel() {
		if(MenuStateMachine.instance.notInTut()) {
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

		} else {
			if(MenuStateMachine.instance.tutStep < MenuStateMachine.TUTORIAL_PHASE.ExtraSpear) {
				levelNow = 0;
			} else {
				levelNow = 2;
			}
		}
		SpawnForLevel();
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
		if(levCapped >= fishLevelOption[whichFishSeq].fishLevelSeq.Length) {
			levCapped = fishLevelOption[whichFishSeq].fishLevelSeq.Length - 1;
			Debug.Log("LEVEL DEFINITION MISSING FOR seq " + whichFishSeq + " on levelNow: " + levelNow);
		}
		for(int i=0;i<fishLevelOption[whichFishSeq].fishLevelSeq[levCapped].fishKinds.Length;i++) {
			int howMany = fishLevelOption[whichFishSeq].fishLevelSeq[levCapped].fishKinds[i].howMany;
			for(int ii=0;ii<howMany;ii++) {
				GameObject whichPrefab = basicTypes[(int)(fishLevelOption[whichFishSeq].fishLevelSeq[levCapped].fishKinds[i].fishType)];
				GameObject GOFish = (GameObject)GameObject.Instantiate(whichPrefab);
				FishMoverBasic fmbScript = GOFish.GetComponent<FishMoverBasic>();
				GOFish.transform.position =
					SeaBounds.instance.randPosBandBias(fmbScript.depthBiasOdds,
					                                   fmbScript.shallowPerc,
					                                   fmbScript.deepPerc);
				GOFish.name = "Fish"+ whichPrefab.name +" " + (ii+1);
				fishList.Add(GOFish);
				fmbScript.aiMode = fishLevelOption[whichFishSeq].fishLevelSeq[levCapped].fishKinds[i].moveStyle;
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

	void Awake() {
		instance = this;
	}

	// Use this for initialization
	void Start () {
		fishList = new List<GameObject>();
		GameObject weatherObj = GameObject.Find("Environment");
		weatherMaster = weatherObj.GetComponent<WeatherController>();
		SpawnForLevel();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
