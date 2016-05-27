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

	void NextLevel() {
		levelNow++;
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

	void SpawnForLevel() {
		showDayText.showDay(levelNow);
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
