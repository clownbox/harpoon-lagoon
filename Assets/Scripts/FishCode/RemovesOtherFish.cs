using UnityEngine;
using System.Collections;

public class RemovesOtherFish : MonoBehaviour {
	HarpoonablePassingMonster hpmScript;
	AnimModelListOneTime animateToRemove;

	void Start() {
		hpmScript = GetComponent<HarpoonablePassingMonster>();
		animateToRemove = GetComponentInChildren<AnimModelListOneTime>();
	}

	void OnTriggerEnter(Collider other) {
		FishMoverBasic fmbScript = other.gameObject.GetComponentInParent<FishMoverBasic>();
		if(hpmScript.IsAlive() && fmbScript && fmbScript.IsAlive()) {
			fmbScript.Die(true);
			hpmScript.hideBoth();
			animateToRemove.enabled = true;
			FMODUnity.RuntimeManager.PlayOneShot("event:/fish_dead");
		}
	
	}
}
