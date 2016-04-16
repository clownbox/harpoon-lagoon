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
	public FishMoverBasic.FishMove aiOverride = FishMoverBasic.FishMove.FISH_AI_TYPES;
	[HideInInspector] 
	public float probabilityRangeMin;
}

[Serializable]
public class FishRatiosList
{
	public FishTypeAndProbability[] fishSet;
	public string setName;
	public FishMoverBasic.FishMove aiMode;
	public int howManyToKeep = 5;
}

public class FishSpawnRefillTank : MonoBehaviour {
	public FishRatiosList[] fishScalingList;
	public int fishListNow = 0;
	/*public Transform alignTop;
	public Transform alignBot;*/
	public bool lineUpFish = true;
	public Text aiFishText;
	public Text fishSetText;
	float totalWeight = 0.0f;

	public static FishSpawnRefillTank instance;

	public static FishMoverBasic.FishMove defaultAI = FishMoverBasic.FishMove.HORIZONTAL_LINE;

	public bool debugOutput = false;

	List<GameObject> fishList;
	
	public void Restart() {
		foreach(GameObject GOFish in fishList) {
			if(GOFish) {
				Destroy(GOFish);
			}
		}
		fishList.Clear();
		SpawnUpToCap(true);
	}

	public void cycleFishAIDefault() {
		defaultAI++;
		if((int)defaultAI >= (int)FishMoverBasic.FishMove.FISH_AI_TYPES) {
			defaultAI = (FishMoverBasic.FishMove)0;
		}

		SetAllFishAI(defaultAI);
	}

	public void toggleFishLines() {
		lineUpFish = !lineUpFish;
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
		for(int i = 0; i < fishScalingList[fishListNow].fishSet.Length; i++) {
			fishScalingList[fishListNow].fishSet[i].probabilityRangeMin = totalWeight;
			totalWeight += fishScalingList[fishListNow].fishSet[i].fishRelativeRatioWeight;
			totalMin += fishScalingList[fishListNow].fishSet[i].minCount;
			if(fishScalingList[fishListNow].fishSet[i].minCount > fishScalingList[fishListNow].fishSet[i].maxCount) {
				FishMoverBasic.FishBreed thisKind = fishScalingList[fishListNow].fishSet[i].fishTypePrefab.GetComponent<FishMoverBasic>().myKind;
				Debug.LogWarning(name + ":FishListWeightsSetup.cs, MIN SHOULD NOT EXCEED MAX (type "+thisKind+")");
			}
		}

		if(totalMin > fishScalingList[fishListNow].howManyToKeep) {
			Debug.LogWarning(name + ":"+totalMin +" > "+ fishScalingList[fishListNow].howManyToKeep+", MIN OF ALL FISH COMBINED EXCEEDS TOTAL NUMBER OF FISH TO MAINTAIN");
		}

		for(int i = 0; i < fishScalingList[fishListNow].fishSet.Length; i++) {
			fishScalingList[fishListNow].fishSet[i].fishRelativeRatioWeight /= totalWeight;
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

		for(int ii=fishList.Count;ii<fishScalingList[fishListNow].howManyToKeep;ii++) {

			forceKind = FishMoverBasic.FishBreed.NONE;
			for(int iii = 0; iii < fishScalingList[fishListNow].fishSet.Length; iii++) {
				FishMoverBasic.FishBreed thisKind = fishScalingList[fishListNow].fishSet[iii].fishTypePrefab.GetComponent<FishMoverBasic>().myKind;

				if(fishScalingList[fishListNow].fishSet[iii].minCount > typesCount[ (int)thisKind ] ) {
					forceKind = thisKind;
					if(debugOutput) {
						Debug.Log("Forcing kind to meet minimum: "+forceKind+" ("+
							typesCount[(int)thisKind] + " of "+ fishScalingList[fishListNow].fishSet[iii].minCount+")");
					}
				}
			}
			if(forceKind == FishMoverBasic.FishBreed.NONE) {
				spawnType = -1;
				int safetyRetryLimit = 100;

				while(spawnType == -1 && safetyRetryLimit-->0) {
					float diceRoll = UnityEngine.Random.Range(0.0f, 1.0f);

					for(int i = 0; i < fishScalingList[fishListNow].fishSet.Length; i++) {
						if(fishScalingList[fishListNow].fishSet[i].fishRelativeRatioWeight < diceRoll) {
							FishMoverBasic.FishBreed thisKind = fishScalingList[fishListNow].fishSet[i].fishTypePrefab.GetComponent<FishMoverBasic>().myKind;

							if(typesCount[(int)thisKind] < fishScalingList[fishListNow].fishSet[i].maxCount) {
								spawnType = i;
							} else if(debugOutput) {
								Debug.Log("Spawning another " +thisKind+" fish would exceed max count ("+
									typesCount[(int)thisKind] + " of "+ fishScalingList[fishListNow].fishSet[i].maxCount+")");
							}
							break;
						}
					}
				}
				if(safetyRetryLimit <= 0) {
					Debug.Log(name + ":FishSpawnRefillTank, UNABLE TO SPAWN NEW FISH WITHIN MAX LIMITS SET");
					break;
				}
			} else {
				spawnType = (int)forceKind;
			}

			// Debug.Log(spawnType);
			if(spawnType >= 0) {
				typesCount[spawnType]++;

				GameObject GOFish = (GameObject)GameObject.Instantiate(fishScalingList[fishListNow].fishSet[spawnType].fishTypePrefab);
				FishMoverBasic fmbScript = GOFish.GetComponent<FishMoverBasic>();
				GOFish.transform.position =
				SeaBounds.instance.randPosBandBias(fmbScript.depthBiasOdds,
					fmbScript.shallowPerc,
					fmbScript.deepPerc,
					isFirstFillingSoGoAnywhere);
				GOFish.name = "Fish" + fishScalingList[fishListNow].fishSet[spawnType].fishTypePrefab.name + " " + (ii + 1);
				if(fishScalingList[fishListNow].fishSet[spawnType].aiOverride !=
				   FishMoverBasic.FishMove.FISH_AI_TYPES) {
					fmbScript.aiMode = fishScalingList[fishListNow].fishSet[spawnType].aiOverride;
				} else {
					fmbScript.aiMode = defaultAI;
				}
				fishList.Add(GOFish);
			}
		}
	}

	// Use this for initialization
	void Start () {
		instance = this;
		fishList = new List<GameObject>();
		FishListWeightsSetup();
		SpawnUpToCap(true);

		fishListNow--; // HACK NOTE: since calling cycle func to update button label will ++ it
		CycleFishSet();
	}

	public void SetAllFishAI(FishMoverBasic.FishMove toMode) {
		defaultAI = toMode;
		for(int iii = 0; iii < fishList.Count; iii++) {
			fishList[iii].GetComponent<FishMoverBasic>().setAIMode(toMode);
		}

		aiFishText.text = ""+toMode;
	}

	public void CycleFishSet() {
		fishListNow++;
		if(fishListNow >= fishScalingList.Length) {
			fishListNow = 0;
		}

		int fishMinSum = 0, fishMaxSum = 0;

		for(int iii = 0; iii < fishScalingList[fishListNow].fishSet.Length; iii++) {
			fishMinSum += fishScalingList[fishListNow].fishSet[iii].minCount;
			fishMaxSum += fishScalingList[fishListNow].fishSet[iii].maxCount;
		}

		if(fishScalingList[fishListNow].howManyToKeep < fishMinSum ||
			fishScalingList[fishListNow].howManyToKeep > fishMaxSum) {
			fishScalingList[fishListNow].howManyToKeep = (int)((fishMinSum + fishMaxSum) / 2);
		}

		FishListWeightsSetup();

		fishSetText.text = fishScalingList[fishListNow].setName;
		SetAllFishAI( fishScalingList[fishListNow].aiMode );
		Restart();
	}

	/*void Update () {
		Debug.DrawLine(alignTop.transform.position, alignBot.transform.position, Color.white);
	}*/
}
