using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Collections.Generic;

[Serializable]
public class FishTypeAndProbability
{
	public GameObject fishTypePrefab;
	public int minCount = 0;
	public int maxCount = 3;
	public float fishRelativeRatioWeight;
	[HideInInspector] 
	public float probabilityRangeMin;
}

public class FishSpawnRefillTank : MonoBehaviour {
	public FishTypeAndProbability[] fishScalingList;
	public Transform alignTop;
	public Transform alignBot;
	public bool lineUpFish = true;
	// public Text lineUpFishText;
	public int howManyToKeep = 5;
	float totalWeight = 0.0f;
	private bool firstTimeFillingTank = true;

	public static FishSpawnRefillTank instance;

	public bool debugOutput = false;

	List<GameObject> fishList;
	
	public void Restart() {
		foreach(GameObject GOFish in fishList) {
			if(GOFish) {
				Destroy(GOFish);
			}
		}
		SpawnUpToCap(true);
	}

	public void toggleFishLines() {
		lineUpFish = !lineUpFish;
		// lineUpFishText.text = (lineUpFish ? "lines" : "spread"); 
	}

	public void FishKilledAndOffScreen_Refill(GameObject whichFish) {
		fishList.Remove(whichFish);

		if(ScoreManager.instance.ShowGameOverIfNeeded() == false) {
			SpawnUpToCap();
		}
	}

	void FishListWeightsSetup() {
		totalWeight = 0.0f;

		int totalMin = 0;
		for(int i = 0; i < fishScalingList.Length; i++) {
			fishScalingList[i].probabilityRangeMin = totalWeight;
			totalWeight += fishScalingList[i].fishRelativeRatioWeight;
			totalMin += fishScalingList[i].minCount;
			if(fishScalingList[i].minCount > fishScalingList[i].maxCount) {
				FishMoverBasic.FishBreed thisKind = fishScalingList[i].fishTypePrefab.GetComponent<FishMoverBasic>().myKind;
				Debug.LogWarning(name + ":FishListWeightsSetup.cs, MIN SHOULD NOT EXCEED MAX (type "+thisKind+")");
			}
		}

		if(totalMin > howManyToKeep) {
			Debug.LogWarning(name + ":FishListWeightsSetup.cs, MIN OF ALL FISH COMBINED EXCEEDS TOTAL NUMBER OF FISH TO MAINTAIN");
		}

		for(int i = 0; i < fishScalingList.Length; i++) {
			fishScalingList[i].fishRelativeRatioWeight /= totalWeight;
		}
	}

	void SpawnUpToCap(bool isFirstFillingSoGoAnywhere = false) {
		int[] typesCount = new int[(int)FishMoverBasic.FishBreed.FISH_KINDS];

		for(int iii = 0; iii < (int)FishMoverBasic.FishBreed.FISH_KINDS; iii++) {
			typesCount[iii] = 0;
		}
		for(int iii = 0; iii < fishList.Count; iii++) {
			typesCount[ (int)fishList[iii].GetComponent<FishMoverBasic>().myKind ]++;
		}

		FishMoverBasic.FishBreed forceKind;
		int spawnType = 0;

		for(int ii=fishList.Count;ii<howManyToKeep;ii++) {

			forceKind = FishMoverBasic.FishBreed.NONE;
			for(int iii = 0; iii < fishScalingList.Length; iii++) {
				FishMoverBasic.FishBreed thisKind = fishScalingList[iii].fishTypePrefab.GetComponent<FishMoverBasic>().myKind;

				if(fishScalingList[iii].minCount > typesCount[ (int)thisKind ] ) {
					forceKind = thisKind;
					if(debugOutput) {
						Debug.Log("Forcing kind to meet minimum: "+forceKind+" ("+
							typesCount[(int)thisKind] + " of "+ fishScalingList[iii].minCount+")");
					}
				}
			}
			if(forceKind == FishMoverBasic.FishBreed.NONE) {
				spawnType = -1;
				int safetyRetryLimit = 100;

				while(spawnType == -1 && safetyRetryLimit-->0) {
					float diceRoll = UnityEngine.Random.Range(0.0f, 1.0f);

					for(int i = 0; i < fishScalingList.Length; i++) {
						if(fishScalingList[i].fishRelativeRatioWeight < diceRoll) {
							FishMoverBasic.FishBreed thisKind = fishScalingList[i].fishTypePrefab.GetComponent<FishMoverBasic>().myKind;

							if(typesCount[(int)thisKind] < fishScalingList[i].maxCount) {
								spawnType = i;
							} else if(debugOutput) {
								Debug.Log("Spawning another " +thisKind+" fish would exceed max count ("+
									typesCount[(int)thisKind] + " of "+ fishScalingList[i].maxCount+")");
							}
							break;
						}
					}
				}
				if(safetyRetryLimit <= 0) {
					Debug.LogError(name + ":FishSpawnRefillTank, UNABLE TO SPAWN NEW FISH WITHIN MAX LIMITS SET");
				}
			} else {
				spawnType = (int)forceKind;
			}

			// Debug.Log(spawnType);
			typesCount[spawnType]++;

			GameObject GOFish = (GameObject)GameObject.Instantiate(fishScalingList[spawnType].fishTypePrefab);
			FishMoverBasic fmbScript = GOFish.GetComponent<FishMoverBasic>();
			GOFish.transform.position =
				SeaBounds.instance.randPosBandBias(fmbScript.depthBiasOdds,
					fmbScript.shallowPerc,
					fmbScript.deepPerc,
					isFirstFillingSoGoAnywhere);
			GOFish.name = "Fish"+ fishScalingList[spawnType].fishTypePrefab.name +" " + (ii+1);
			fishList.Add(GOFish);
		}
	}

	// Use this for initialization
	void Start () {
		instance = this;
		fishList = new List<GameObject>();
		FishListWeightsSetup();
		SpawnUpToCap(true);

		// calling twice as lazy way to just keep state + update text
		toggleFishLines();
		toggleFishLines();
	}

	void Update () {
		Debug.DrawLine(alignTop.transform.position, alignBot.transform.position, Color.white);
	}
}
