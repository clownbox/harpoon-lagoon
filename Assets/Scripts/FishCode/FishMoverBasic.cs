using UnityEngine;
using System.Collections;

public class FishMoverBasic : MonoBehaviour {
	public enum FishBreed {STANDARD,SHIFTY,TINYFAST,GOLDEN,FISH_KINDS,NONE};

	public FishBreed myKind; // simpler test for uniqueness when matching
	Vector3 swimmingFrom;
	Vector3 swimmingTo;
	Vector3 rootPos;

	public MeshRenderer preStabbedFish;
	public MeshRenderer postStabbedFish;

	float swimTimeStarted;
	float swimTimeEnd;
	bool seekingGoal;
	bool isDead;

	Transform[] modelVis;

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
	float enoughXToTurn = 0.05f;

	Vector3 recentPushFrom;
	Vector3 recentPushTo;

	IEnumerator WaitBeforeNewGoal() {
		yield return new WaitForSeconds( Random.Range(minDriftTime, maxDriftTime) );
		PickNewGoal();
	}

	void PickNewGoal() {
		if(isDead) { // checking here too in case WaitBeforeNewGoal timer was running when speared
			return;
		}

		seekingGoal = true;
		swimmingFrom = rootPos;
		swimmingTo = SeaBounds.instance.randPosWithinMinMaxRange(swimmingFrom,
			1.3f*WeatherController.weatherSprintDistMult,
			2.9f*WeatherController.weatherSprintDistMult);
		swimTimeStarted = FishTime.time;
		swimTimeEnd = swimTimeStarted + timePerSprint* WeatherController.weatherSprintDelayMult;
	}

	public bool IsAlive() {
		return (isDead == false);
	}

	public void Die() {
		if(isDead == false) {
			isDead = true;

			preStabbedFish.enabled = false;
			postStabbedFish.enabled = true;
		}
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
		Renderer[] rendChild = gameObject.GetComponentsInChildren<Renderer>();
		updateFacingTarget();

		modelVis = new Transform[rendChild.Length];

		for(int i = 0; i < rendChild.Length; i++) {
			modelVis[i] = rendChild[i].transform;
			modelVis[i].rotation = Quaternion.Euler(270.0f, 90.0f + sideToSideFacingFloat, 0.0f);
		}

		isDead = false;
		randPhaseOffset = Random.Range(0.0f,Mathf.PI*2.0f);
		PickNewGoal();

		preStabbedFish.enabled = true;
		postStabbedFish.enabled = false;

		rootPos = transform.position;
		// swimmingTo = SeaBounds.instance.randPos();
	}
	
	// Update is called once per frame
	void Update () {
		if(MenuStateMachine.instance.MenuBlocksAction() || isDead) {
			return;
		}

		float pushRange = 1.5f;
		float pushForce = 1.0f;
		Collider[] nearbyFish = Physics.OverlapSphere(rootPos, pushRange);
		for(int i = 0; i < nearbyFish.Length; i++) {
			FishMoverBasic otherFMB = nearbyFish[i].GetComponentInParent<FishMoverBasic>();
			if(otherFMB == null) {
				continue;
			}
			Vector3 posDiff = transform.position - otherFMB.transform.position;
			posDiff.z = 0.0f;
			float posDist = posDiff.magnitude;
			if(posDist > 0.01f) { // skip self
				float invDistPercForPushForce = (pushRange - posDist) / pushRange;
				invDistPercForPushForce *= invDistPercForPushForce; // sq effect
				Vector3 pushBack = posDiff.normalized * FishTime.deltaTime *
					invDistPercForPushForce * pushForce;

				recentPushFrom = transform.position;
				recentPushTo = otherFMB.transform.position;

				rootPos = SeaBounds.instance.constrainTrunc(rootPos+pushBack);
				swimmingFrom = rootPos;
				float timeLeft = swimTimeEnd - FishTime.time;
				swimTimeStarted = FishTime.time;
				swimTimeEnd = swimTimeStarted + timeLeft;
				swimmingTo = SeaBounds.instance.constrainTrunc(swimmingTo+pushBack);
			}
		}

		updateFacingTarget();

		float wigglePower = Vector3.Distance(swimmingFrom, swimmingTo);
		wiggleOscFakeTime += Time.deltaTime * wigglePower * 1.2f;
		float wiggleOsc = Mathf.Cos( wiggleOscFakeTime ) * 20.0f;
		for(int i = 0; i < modelVis.Length; i++) {
			modelVis[i].rotation = Quaternion.Slerp(modelVis[i].rotation,
				Quaternion.Euler(270.0f, 90.0f + sideToSideFacingFloat + wiggleOsc, 0.0f),
				((Time.deltaTime + FishTime.deltaTime * 2.0f) / 3.0f) * 3.3f);
		}

		if(isDead) {
			return;
		}
		if(seekingGoal) {
			if(FishTime.time > swimTimeEnd) {
				seekingGoal = false;
				StartCoroutine( WaitBeforeNewGoal() );
			} else { // not out of time, still seeking goal
				float percProgress = (FishTime.time-swimTimeStarted)/timePerSprint;
				//float invSq = 1.0f-(1.0f-percProgress)*(1.0f-percProgress); // sprint then smooth slowdown
				float invSq = percProgress; // sprint then smooth slowdown
				rootPos = Vector3.Lerp(swimmingFrom,swimmingTo, invSq);
			}
		} 
		// drift
		Vector3 driftPos = rootPos +
		     Mathf.Cos(FishTime.time / 1.3f + randPhaseOffset) * transform.right * driftX * 10.0f
		      * WeatherController.weatherDriftMultSmoothed
			+
			Mathf.Sin (FishTime.time/4.1f+randPhaseOffset) * transform.up * driftY * 10.0f
			*WeatherController.weatherDriftMultSmoothed;
		transform.position = SeaBounds.instance.constrainTrunc( driftPos );

		if(seekingGoal) {
			Debug.DrawLine(swimmingFrom, swimmingTo, Color.red);
		}
		Debug.DrawLine(transform.position, rootPos, Color.green);

		Debug.DrawLine(recentPushFrom, recentPushTo, Color.cyan);

	} // end of Update()

} // end of class
