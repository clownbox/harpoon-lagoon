using UnityEngine;
using System.Collections;

public class AnimModelListPingPong : MonoBehaviour {
	public Renderer[] models;
	private float animDelay = 0.425f;
	private float animTimeLeft;
	private int frameNow = 0;
	private int frameDir = 1;

	void ForceFrame () {
		for(int i = 0; i < models.Length; i++) {
			models[i].enabled = (i==frameNow);
		}
	}

	void Start () {
		ForceFrame();
		animTimeLeft = animDelay;
	}

	void Update() {
		animTimeLeft -= Time.deltaTime;
		if(animTimeLeft <= 0.0f) {
			animTimeLeft = animDelay;
			frameNow += frameDir;
			if(frameDir > 0 && frameNow==models.Length-1) {
				frameDir *= -1;
			} else if(frameDir < 0 && frameNow==0) {
				frameDir *= -1;
			}
			ForceFrame();
		}
	}

}
