using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

[Serializable]
public class FishTypeAndBaseAndMult
{
	public GameObject fishTypePrefab;
	public float fishBaseCount;
	public float fishExtraPerLevel;
}

public class FishSpawnInfinite : MonoBehaviour {
	public FishTypeAndBaseAndMult[] fishScalingList;
	public static FishSpawnInfinite instance;
	private int levelNow = 0;
	public TextFadeOut showDayText;

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
		GameObject GOFish = (GameObject)GameObject.Instantiate(fishScalingList[fishKind].fishTypePrefab);
		FishMoverBasic fmbScript = GOFish.GetComponent<FishMoverBasic>();
		GOFish.transform.position =
			SeaBounds.instance.randPosBandBias(fmbScript.depthBiasOdds,
				fmbScript.shallowPerc,
				fmbScript.deepPerc);
	}

	void SpawnForLevel() {
		SharkHurry.instance.retreating = true;
		UpdateText();
		RemoveAll();
		totalFishTillRespawn = 0;
		for(int i=0;i<fishScalingList.Length;i++) {
			int howMany = (int)(fishScalingList[i].fishBaseCount +
			                    fishScalingList[i].fishExtraPerLevel * levelNow);
			for(int ii=0;ii<howMany;ii++) {
				GameObject GOFish = (GameObject)GameObject.Instantiate(fishScalingList[i].fishTypePrefab);
				FishMoverBasic fmbScript = GOFish.GetComponent<FishMoverBasic>();
				GOFish.transform.position =
					SeaBounds.instance.randPosBandBias(fmbScript.depthBiasOdds,
					                                   fmbScript.shallowPerc,
					                                   fmbScript.deepPerc);
				GOFish.name = "Fish"+ fishScalingList[i].fishTypePrefab.name +" " + (ii+1);
				fishList.Add(GOFish);
				totalFishTillRespawn++;
			}
		}
	}

	void Awake() {
		instance = this;
	}

	// Use this for initialization
	void Start () {
		fishList = new List<GameObject>();
		SpawnForLevel();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
