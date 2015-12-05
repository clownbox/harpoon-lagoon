using UnityEngine;
using System.Collections;

public class RespawnIfSunkBelow : MonoBehaviour {
	public static GameObject reportKillsTo;
	public bool alreadyCounted = false;

	void Start() {
		if(reportKillsTo == null) {
			reportKillsTo = GameObject.Find("LevelFishSpawner");
		}
	}

	// disabled for now, so working against a fixed set of fish
	// Update is called once per frame
	void Update () {
		if(alreadyCounted == false && SeaBounds.instance.outOfBounds(transform.position)) {
			if(reportKillsTo) {
				reportKillsTo.SendMessage("FishKilledAndOffScreen");
			}
			alreadyCounted = true;
		}
	}
}
