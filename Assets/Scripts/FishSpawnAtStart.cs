using UnityEngine;
using System;
using System.Reflection;

[Serializable]
public class FishTypeAndNum
{
	public GameObject fishTypePrefab;
	public int fishCount;
}

public class FishSpawnAtStart : MonoBehaviour {
	public FishTypeAndNum[] fishSpawnList;

	// Use this for initialization
	void Start () {
		for(int i=0;i<fishSpawnList.Length;i++) {
			for(int ii=0;ii<fishSpawnList[i].fishCount;ii++) {
				GameObject GOFish = (GameObject)GameObject.Instantiate(fishSpawnList[i].fishTypePrefab);
				FishMoverBasic fmbScript = GOFish.GetComponent<FishMoverBasic>();
				GOFish.transform.position =
					SeaBounds.instance.randPosBandBias(fmbScript.depthBiasOdds,
					                                   fmbScript.shallowPerc,
					                                   fmbScript.deepPerc);
				GOFish.name = "Fish"+ fishSpawnList[i].fishTypePrefab.name +" " + (ii+1);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
