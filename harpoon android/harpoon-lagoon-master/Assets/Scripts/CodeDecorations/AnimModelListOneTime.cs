using UnityEngine;
using System.Collections;

public class AnimModelListOneTime : MonoBehaviour {
	public Renderer[] models;
	private float animDelay = 0.06f;
	private float animTimeLeft;
	private int frameNow = 0;
	public AnimModelToggle passBackControlTo;

	public void ForceFrame () {
		for(int i = 0; i < models.Length; i++) {
			models[i].enabled = (i==frameNow);
		}
	}

	void Start () {
		frameNow = -1;
		ForceFrame();
		frameNow++;
		animTimeLeft = animDelay;
		enabled = false;
	}

	void Update() {
		animTimeLeft -= Time.deltaTime;
		if(animTimeLeft <= 0.0f) {
			animTimeLeft = animDelay;
			frameNow++;
			ForceFrame();
			if(frameNow==models.Length) {
				frameNow = 0;
				passBackControlTo.restore();
				enabled = false;
			}
		}
	}

}
