using UnityEngine;
using System.Collections;

public class SharkHurry : MonoBehaviour {
	public Transform headPopInLoc;
	public static SharkHurry instance;

	float driftOut = 0.0f;
	public float peekOutSpeed = 0.4f;
	public bool retreating = false;

	public float timeUntilBeginsToPeek = 5.0f;
	float timeUntilPeek = 0.4f;

	public bool sharkReady = false;

	public void RestartWait() {
		timeUntilPeek = timeUntilBeginsToPeek;
		driftOut = 0.0f;
		sharkReady = false;
		retreating = false;
		UpdatePos();
	}

	public void UpdatePos() {
		transform.position = headPopInLoc.transform.position
			+ Vector3.right * (1.0f-driftOut);
	}

	void Awake() {
		instance = this;
	}

	// Use this for initialization
	void Start () {
		RestartWait();
	}

	public void retreatIfNotReady() {
		if(sharkReady == false) {
			retreating = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(MenuStateMachine.instance.tutStep == MenuStateMachine.TUTORIAL_PHASE.Shark) {
			if(timeUntilPeek > 0.2f) {
				timeUntilPeek = 0.2f;
			}
		} else if(MenuStateMachine.instance.notInTut()==false) { // no shark step yet
			retreating = true;
		}

		if(retreating) {
			sharkReady = false;
			driftOut -= Time.deltaTime * peekOutSpeed * 3.0f;
			if(driftOut < 0.0f) {
				RestartWait();
			}
			UpdatePos();
			return;
		}

		if(sharkReady) {
			return;
		}

		if(timeUntilPeek > 0.0f) {
			timeUntilPeek -= Time.deltaTime;
			return;
		}

		driftOut += Time.deltaTime * peekOutSpeed;
		if(driftOut > 1.0f) {
			driftOut = 1.0f;
			sharkReady = true;
		}
		UpdatePos();
	}
}
