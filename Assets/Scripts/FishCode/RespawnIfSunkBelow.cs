using UnityEngine;
using System.Collections;

public class RespawnIfSunkBelow : MonoBehaviour {
	public static GameObject reportKillsTo;
	public bool alreadyCounted = false;
	public bool isSpearNotFish = false;

	void Start() {
		if(reportKillsTo == null) {
			reportKillsTo = GameObject.Find("LevelFishSpawner");
		}
	}

	public void CountFish() {
		if(alreadyCounted == false) {
			reportKillsTo.SendMessage("FishKilledAndOffScreen_Refill", gameObject);
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
			if(SeaBounds.instance.spearOutOfBounds(transform.position)) {
				ScoreManager.instance.SpearOffScreen(gameObject);
				alreadyCounted = true;
			}
		} else {
			if(SeaBounds.instance.outOfBounds(transform.position)) {
				CountFish();
			}
		}
	}
}
