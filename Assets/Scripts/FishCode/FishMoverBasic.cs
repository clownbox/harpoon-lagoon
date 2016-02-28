﻿using UnityEngine;
using System.Collections;

public class FishMoverBasic : MonoBehaviour {
	public enum FishBreed {STANDARD,SHIFTY,TINYFAST,GOLDEN,FISH_KINDS,NONE};

	public FishBreed myKind; // simpler test for uniqueness when matching
	Vector3 swimmingFrom;
	Vector3 swimmingTo;

	float swimTimeStarted;
	float swimTimeEnd;
	bool seekingGoal;
	bool isDead;

	Transform modelVis;

	public int scoreValue;

	public float timePerSprint = 3.9f;
	public float minDriftTime = 0.7f;
	public float maxDriftTime = 1.7f;
	public float driftX = 0.3f;
	public float driftY = 0.2f;

	public float depthBiasOdds = 0.5f;
	public float shallowPerc = 0.3f;
	public float deepPerc = 0.85f;

	float sideToSideFacingFloat = 0.0f;
	float wiggleOscFakeTime = 0.0f;

	float randPhaseOffset;
	float lastTurnX;
	float enoughXToTurn = 0.15f;

	IEnumerator WaitBeforeNewGoal() {
		yield return new WaitForSeconds( Random.Range(minDriftTime, maxDriftTime) );
		PickNewGoal();
	}

	void PickNewGoal() {
		if(isDead) { // checking here too in case WaitBeforeNewGoal timer was running when speared
			return;
		}

		seekingGoal = true;
		swimmingFrom = transform.position;
		swimmingTo = SeaBounds.instance.randPosWithinMinMaxRange(swimmingFrom,1.3f,2.9f,this);
		swimTimeStarted = FishTime.time;
		swimTimeEnd = swimTimeStarted + timePerSprint;
	}

	public bool IsAlive() {
		return (isDead == false);
	}

	public void Die() {
		isDead = true;
	}

	void updateFacingTarget() {
		float diffX = transform.position.x - lastTurnX;
		if(Mathf.Abs(diffX) > enoughXToTurn) {
			if(diffX > 0.0f) {
				sideToSideFacingFloat = 0.0f;
			} else {
				sideToSideFacingFloat = 180.0f;
			}
			lastTurnX = transform.position.x;
		}
	}

	// Use this for initialization
	void Start () {
		Renderer rendChild = gameObject.GetComponentInChildren<Renderer>();
		modelVis = rendChild.transform;

		updateFacingTarget();
		modelVis.rotation = Quaternion.Euler(270.0f, 90.0f + sideToSideFacingFloat, 0.0f);

		isDead = false;
		randPhaseOffset = Random.Range(0.0f,Mathf.PI*2.0f);
		PickNewGoal();
	}
	
	// Update is called once per frame
	void Update () {
		if(MenuStateMachine.instance.MenuBlocksAction() || isDead) {
			return;
		}

		updateFacingTarget();

		float wigglePower = Vector3.Distance(swimmingFrom, swimmingTo);
		wiggleOscFakeTime += Time.deltaTime * wigglePower * 1.2f;
		float wiggleOsc = Mathf.Cos( wiggleOscFakeTime ) * 20.0f;
		modelVis.rotation = Quaternion.Slerp(modelVis.rotation,
			Quaternion.Euler(270.0f, 90.0f + sideToSideFacingFloat + wiggleOsc, 0.0f),
			Time.deltaTime * 2.3f);

		if(isDead) {
			return;
		}
		if(seekingGoal) {
			if(FishTime.time > swimTimeEnd) {
				seekingGoal = false;
				StartCoroutine( WaitBeforeNewGoal() );
			} else { // not out of time, still seeking goal
				float percProgress = (FishTime.time-swimTimeStarted)/timePerSprint;
				float invSq = 1.0f-(1.0f-percProgress)*(1.0f-percProgress); // sprint then smooth slowdown
				transform.position = Vector3.Lerp(swimmingFrom,swimmingTo, invSq);
			}
		} else { // no goal, drift
			Vector3 driftPos = transform.position +
				Mathf.Cos (FishTime.time/1.3f+randPhaseOffset) * transform.right * driftX * FishTime.deltaTime +
				Mathf.Cos (FishTime.time/4.1f+randPhaseOffset) * transform.up * driftY * FishTime.deltaTime;
			transform.position = SeaBounds.instance.constrain( driftPos );
		} // end of else/drift
	} // end of Update()

} // end of class
