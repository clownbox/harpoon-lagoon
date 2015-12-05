using UnityEngine;
using System;
using System.Reflection;

[Serializable]
public class FishTypeAndBaseAndMult
{
	public GameObject fishTypePrefab;
	public float fishBaseCount;
	public float fishExtraPerLevel;
}

public class FishSpawnInfinite : MonoBehaviour {
	public FishTypeAndBaseAndMult[] fishScalingList;
	private int levelNow = 0;

	private int totalFishTillRespawn = 0;

	public void FishKilledAndOffScreen() {
		totalFishTillRespawn--;
		if(totalFishTillRespawn<=0) {
			levelNow++;
			SpawnForLevel();
		}
	}

	void SpawnForLevel() {
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
				totalFishTillRespawn++;
			}
		}
	}

	// Use this for initialization
	void Start () {
		SpawnForLevel();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
