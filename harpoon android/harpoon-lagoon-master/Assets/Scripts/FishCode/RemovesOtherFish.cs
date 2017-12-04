using UnityEngine;
using System.Collections;

public class RemovesOtherFish : MonoBehaviour {
	public AnimModelToggle amtScript;
	AnimModelListOneTime animateToRemove;

	void Start() {
		animateToRemove = GetComponentInChildren<AnimModelListOneTime>();
	}

	public void PlayAttackAnim() {
		amtScript.hideBoth();
		animateToRemove.enabled = true;
		animateToRemove.ForceFrame();
	}

	/*void OnTriggerEnter(Collider other) {
		FishMoverBasic fmbScript = other.gameObject.GetComponentInParent<FishMoverBasic>();
		if(fmbScript && fmbScript.IsAlive()) {
			fmbScript.Die(true);
			PlayAttackAnim();
			FMODUnity.RuntimeManager.PlayOneShot("event:/fish_dead");
		}
	
	}*/
}
