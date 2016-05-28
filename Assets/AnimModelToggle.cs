using UnityEngine;
using System.Collections;

public class AnimModelToggle : MonoBehaviour {
	public Renderer model1;
	public Renderer model2;
	private float animDelay = 0.85f;
	private float animTimeLeft;
	private bool isFrame1 = false;

	void ShowFrame () {
		model1.enabled = (isFrame1);
		model2.enabled = (isFrame1==false);
	}
	
	void Start () {
		ShowFrame();
		animTimeLeft = animDelay;
	}

	void Update() {
		animTimeLeft -= Time.deltaTime;
		if(animTimeLeft <= 0.0f) {
			animTimeLeft = animDelay;
			isFrame1 = !isFrame1;
			ShowFrame();
		}
	}

}
