using UnityEngine;
using System.Collections;

public class RespawnIfSunkBelow : MonoBehaviour {
	public static GameObject reportKillsTo;
	public bool alreadyCounted = false;
	public bool isSpearNotFish = false;
	HarpoonDrag harpoonRefIfIAmOne;

	void Start() {
		if(reportKillsTo == null) {
			reportKillsTo = GameObject.Find("LevelFishSpawner");
		}
		harpoonRefIfIAmOne = GetComponent<HarpoonDrag>();
	}

	public void CountFish() {
		if(alreadyCounted == false) {
			// _Refill as in FishKilledAndOffScreen_Refill is for FishSpawnRefillTank
			reportKillsTo.SendMessage("FishKilledAndOffScreen"/*_Refill"*/, gameObject);
			alreadyCounted = true;
		}
	}

	// disabled for now, so working against a fixed set of fish
	// Update is called once per frame
	void Update () {
		if(alreadyCounted) {
			return;
		}

		if(isSpearNotFish) {
			if(SeaBounds.instance.spearOutOfBounds(transform.position,harpoonRefIfIAmOne)) {
				ScoreManager.instance.SpearStartReturning(gameObject);
				alreadyCounted = true;
			}
		} else {
			if(SeaBounds.instance.outOfBounds(transform.position)) {
				CountFish();
			}
		}
	}
}
