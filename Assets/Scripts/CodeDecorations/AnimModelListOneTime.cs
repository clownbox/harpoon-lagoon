using UnityEngine;
using System.Collections;

public class AnimModelListOneTime : MonoBehaviour {
	public Renderer[] models;
	private float animDelay = 0.1f;
	private float animTimeLeft;
	private int frameNow = 0;
	public HarpoonablePassingMonster passBackControlTo;

	void ForceFrame () {
		for(int i = 0; i < models.Length; i++) {
			models[i].enabled = (i==frameNow);
		}
	}

	void Start () {
		ForceFrame();
		animTimeLeft = animDelay;
		enabled = false;
	}

	void Update() {
		animTimeLeft -= Time.deltaTime;
		if(animTimeLeft <= 0.0f) {
			animTimeLeft = animDelay;
			frameNow++;
			ForceFrame();
			if(frameNow==models.Length-1) {
				frameNow = 0;
				passBackControlTo.restore();
				enabled = false;
			}
		}
	}

}
