using UnityEngine;
using System.Collections;

public class FishMoverBasic : MonoBehaviour {
	public enum FishBreed {STANDARD,SHIFTY,TINYFAST,GOLDEN,BLOWFISH,SWORDFISH,FISH_KINDS,NONE};

	public enum FishMove
	{
		STANDARD_SPREAD,
		HORIZONTAL_LINE,
		VERTICAL_LINE,
		CIRCLE_CW,
		CIRCLE_CCW,
		FISH_AI_TYPES
	};

	public FishMove aiMode = FishMove.CIRCLE_CCW;

	public FishBreed myKind; // simpler test for uniqueness when matching
	Vector3 swimmingFrom;
	Vector3 swimmingTo;
	Vector3 rootPos;

	public Vector3 diedPos;

	public GameObject preStabbedFish;
	public MeshRenderer postStabbedFish;

	float swimTimeStarted;
	float swimTimeEnd;
	bool seekingGoal;
	bool isDead;

	public bool stopsHarpoon = false; // used by blowfish

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

	public bool stolenByShark = false;

	float sideToSideFacingFloat = 0.0f;
	float wiggleOscFakeTime = 0.0f;

	float randPhaseOffset;
	float lastTurnX;
	float enoughXToTurn = 0.05f;

	Vector3 recentPushFrom;
	Vector3 recentPushTo;

	IEnumerator WaitBeforeNewGoal() {
		if(aiMode == FishMove.STANDARD_SPREAD) {
			yield return new WaitForSeconds(Random.Range(minDriftTime, maxDriftTime));
		} else {
			yield return new WaitForSeconds(0.1f);
		}
		PickNewGoal();
	}

	void PutSwimToOnTargetLine() {
		/*if(FishSpawnRefillTank.instance.lineUpFish) {
			swimmingTo = HarpoonDrag.getProjectedPointOnLine(swimmingTo,
				FishSpawnRefillTank.instance.alignTop.position,
				FishSpawnRefillTank.instance.alignBot.position);
		}*/
	}

	void PickNewGoal() {
		if(isDead) { // checking here too in case WaitBeforeNewGoal timer was running when speared
			return;
		}

		seekingGoal = true;
		swimmingFrom = rootPos;

		switch(aiMode) {
		case FishMove.STANDARD_SPREAD:
			swimmingTo = SeaBounds.instance.randPosWithinMinMaxRange(swimmingFrom,
				1.3f*WeatherController.weatherSprintDistMult,
				2.9f*WeatherController.weatherSprintDistMult);
			PutSwimToOnTargetLine();
			break;
		case FishMove.HORIZONTAL_LINE:
			swimmingTo = rootPos;
			if(swimmingFrom.x > SeaBounds.instance.middleX) {
				swimmingTo.x = SeaBounds.instance.left;
			} else {
				swimmingTo.x = SeaBounds.instance.right;
			}
			break;
		case FishMove.VERTICAL_LINE:
			swimmingTo = rootPos;
			if(swimmingFrom.y < SeaBounds.instance.centerY) {
				swimmingTo.y = SeaBounds.instance.top;
			} else {
				swimmingTo.y = SeaBounds.instance.bottom;
			}
			break;
		case FishMove.CIRCLE_CW:
			swimmingTo = rootPos;
			if(swimmingFrom.y < SeaBounds.instance.centerY) {
				if(swimmingFrom.x > SeaBounds.instance.middleX) {
					swimmingTo.x = SeaBounds.instance.leftish();
				} else {
					swimmingTo.y = SeaBounds.instance.topish();
				}
			} else {
				if(swimmingFrom.x < SeaBounds.instance.middleX) {
					swimmingTo.x = SeaBounds.instance.rightish();
				} else {
					swimmingTo.y = SeaBounds.instance.bottomish();
				}
			}
			break;
		case FishMove.CIRCLE_CCW:
			swimmingTo = rootPos;
			if(swimmingFrom.y < SeaBounds.instance.centerY) {
				if(swimmingFrom.x < SeaBounds.instance.middleX) {
					swimmingTo.x = SeaBounds.instance.rightish();
				} else {
					swimmingTo.y = SeaBounds.instance.topish();
				}
			} else {
				if(swimmingFrom.x > SeaBounds.instance.middleX) {
					swimmingTo.x = SeaBounds.instance.leftish();
				} else {
					swimmingTo.y = SeaBounds.instance.bottomish();
				}
			}
			break;

		}

		if(MenuStateMachine.instance.tutStep == MenuStateMachine.TUTORIAL_PHASE.SpearThree) {
			swimmingFrom.x = SeaBounds.instance.middleX;
			swimmingTo.x = SeaBounds.instance.middleX;
		}

		swimTimeStarted = FishTime.time;
		swimTimeEnd = swimTimeStarted + timePerSprintBasedOnAIMode()* WeatherController.weatherSprintDelayMult;
	}

	private float timePerSprintBasedOnAIMode() {
		if(aiMode != FishMove.STANDARD_SPREAD) {
			return timePerSprint * 4.5f;
		} else {
			return timePerSprint;
		}
	}

	public bool IsAlive() {
		return (isDead == false);
	}

	public void Die(bool vanishSelf = false) {
		if(isDead == false) {
			diedPos = transform.position;
			isDead = true;

			preStabbedFish.SetActive(false);
			postStabbedFish.enabled = true;

			if(vanishSelf) {
				FishSpawnInfinite.instance.FishKilledAndOffScreen(gameObject);
				Destroy(gameObject);
			}
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

		// aiMode = FishSpawnRefillTank.defaultAI;
		setAIMode( (FishMove)Mathf.Floor( Random.Range(0,(int)FishMove.FISH_AI_TYPES)) );

		for(int i = 0; i < rendChild.Length; i++) {
			modelVis[i] = rendChild[i].transform;
			modelVis[i].rotation = Quaternion.Euler(270.0f, 90.0f + sideToSideFacingFloat, 0.0f);
		}

		isDead = false;
		randPhaseOffset = Random.Range(0.0f,Mathf.PI*2.0f);

		preStabbedFish.SetActive(true);
		postStabbedFish.enabled = false;

		if(aiMode == FishMove.VERTICAL_LINE) {
			transform.position = SeaBounds.instance.randPos();
		}

		swimmingFrom = swimmingTo = rootPos = transform.position;

		PickNewGoal();

		PutSwimToOnTargetLine();

		// swimmingTo = SeaBounds.instance.randPos();
	}

	public void setAIMode( FishMove newMode ) {
		aiMode = newMode;
		transform.position = SeaBounds.instance.randPos();
		swimmingFrom = swimmingTo = rootPos = transform.position;
		PickNewGoal();
	}
	
	// Update is called once per frame
	void Update () {
		if(MenuStateMachine.instance.MenuBlocksAction() || isDead) {
			return;
		}

		float pushRange = 1.5f;
		float pushForce = 1.0f;

		if(aiMode != FishMove.STANDARD_SPREAD) {
			pushRange = 0.1f;
			pushForce = 0.1f;
		} /*else if(FishSpawnRefillTank.instance.lineUpFish) {
			pushRange = 0.15f;
			pushForce = 0.35f;
		}*/

		Collider[] nearbyFish = Physics.OverlapSphere(rootPos, pushRange);
		for(int i = 0; i < nearbyFish.Length; i++) {
			FishMoverBasic otherFMB = nearbyFish[i].GetComponentInParent<FishMoverBasic>();
			if(otherFMB == null) {
				continue;
			}
			Vector3 posDiff = transform.position - otherFMB.transform.position;
			posDiff.z = 0.0f;
			float posDist = posDiff.magnitude;
			float pushBackRad = 0.65f;
			if(posDist > 0.01f) { // skip self
				if(posDist < pushBackRad) {
					float smoothK = 0.95f;
					rootPos = SeaBounds.instance.constrainTrunc(otherFMB.transform.position
						+ posDiff.normalized * pushBackRad) * (1.0f-smoothK) +
						rootPos * smoothK;

					swimmingFrom = rootPos;
					float timeLeft = swimTimeEnd - FishTime.time;
					swimTimeStarted = FishTime.time;
					swimTimeEnd = swimTimeStarted + timeLeft;
					swimmingTo = SeaBounds.instance.constrainTrunc(swimmingTo
						+ posDiff.normalized * pushBackRad) * (1.0f-smoothK) +
						swimmingTo * smoothK;
				}
				/*float invDistPercForPushForce = (pushRange - posDist) / pushRange;
				// invDistPercForPushForce *= invDistPercForPushForce; // sq effect
				Vector3 pushBack = Quaternion.AngleAxis(80,Vector3.forward) *
									posDiff.normalized * FishTime.deltaTime *
				                  invDistPercForPushForce * pushForce;

				recentPushFrom = transform.position;
				recentPushTo = otherFMB.transform.position;

				rootPos = SeaBounds.instance.constrainTrunc(rootPos + pushBack);
				swimmingFrom = rootPos;
				float timeLeft = swimTimeEnd - FishTime.time;
				swimTimeStarted = FishTime.time;
				swimTimeEnd = swimTimeStarted + timeLeft;
				swimmingTo = SeaBounds.instance.constrainTrunc(swimmingTo + pushBack);
				PutSwimToOnTargetLine();*/
			}
		}

		if(MenuStateMachine.instance.tutStep == MenuStateMachine.TUTORIAL_PHASE.SpearThree) {
			swimmingFrom.x = SeaBounds.instance.middleX;
			swimmingTo.x = SeaBounds.instance.middleX;
		}

		updateFacingTarget();

		float wigglePower = Vector3.Distance(swimmingFrom, swimmingTo);
		wiggleOscFakeTime += Time.deltaTime * wigglePower * 1.2f;
		float wiggleOsc = Mathf.Cos( wiggleOscFakeTime ) * 20.0f * 0.4f;
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
				float percProgress = (FishTime.time-swimTimeStarted)/timePerSprintBasedOnAIMode();
				//float invSq = 1.0f-(1.0f-percProgress)*(1.0f-percProgress); // sprint then smooth slowdown
				float invSq = percProgress; // sprint then smooth slowdown
				rootPos = Vector3.Lerp(swimmingFrom,swimmingTo, invSq);
			}

			if(aiMode != FishMove.STANDARD_SPREAD) {
				float distToGoal = Vector3.Distance(swimmingTo, rootPos);
				if(distToGoal < 0.01f) {
					seekingGoal = false;
					StartCoroutine( WaitBeforeNewGoal() );
				}
			}

		} 
		// drift
		Vector3 driftPos = rootPos +
		     Mathf.Cos(FishTime.time / 1.3f + randPhaseOffset) * transform.right * driftX * 5.0f
		      * WeatherController.weatherDriftMultSmoothed
			+
			Mathf.Sin (FishTime.time/4.1f+randPhaseOffset) * transform.up * driftY * 5.0f
			*WeatherController.weatherDriftMultSmoothed;
		transform.position = SeaBounds.instance.constrainTrunc( driftPos );

		if(seekingGoal) {
			Debug.DrawLine(swimmingFrom, swimmingTo, Color.red);
		}
		Debug.DrawLine(transform.position, rootPos, Color.green);

		Debug.DrawLine(recentPushFrom, recentPushTo, Color.cyan);

	} // end of Update()

} // end of class
