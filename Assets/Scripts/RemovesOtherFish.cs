using UnityEngine;
using System.Collections;

public class RemovesOtherFish : MonoBehaviour {
	HarpoonablePassingMonster hpmScript;

	void Start() {
		hpmScript = GetComponent<HarpoonablePassingMonster>();
	}

	void OnTriggerEnter(Collider other) {
		FishMoverBasic fmbScript = other.gameObject.GetComponentInParent<FishMoverBasic>();
		if(hpmScript.IsAlive() && fmbScript && fmbScript.IsAlive()) {
			fmbScript.Die(true);
		}
	
	}
}
