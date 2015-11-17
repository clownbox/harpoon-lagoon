using UnityEngine;
using System.Collections;

public class FishMoverBasic : MonoBehaviour {
	Vector3 swimmingFrom;
	Vector3 swimmingTo;
	float swimTimeStarted;
	float swimTimeEnd;
	bool seekingGoal;
	bool isDead;
	public float timePerSprint = 3.9f;
	public float minDriftTime = 0.7f;
	public float maxDriftTime = 1.7f;
	public float driftX = 0.3f;
	public float driftY = 0.2f;

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
		swimmingTo = SeaBounds.instance.randPosWithinMinMaxRange(swimmingFrom,1.3f,2.9f);
		swimTimeStarted = Time.time;
		swimTimeEnd = swimTimeStarted + timePerSprint;
	}

	public bool IsAlive() {
		return (isDead == false);
	}

	public void Die() {
		isDead = true;
	}

	// Use this for initialization
	void Start () {
		isDead = false;
		PickNewGoal();
	}
	
	// Update is called once per frame
	void Update () {
		if(isDead) {
			return;
		}
		if(seekingGoal) {
			if(Time.time > swimTimeEnd) {
				seekingGoal = false;
				StartCoroutine( WaitBeforeNewGoal() );
			} else { // not out of time, still seeking goal
				float percProgress = (Time.time-swimTimeStarted)/timePerSprint;
				float invSq = 1.0f-(1.0f-percProgress)*(1.0f-percProgress); // sprint then smooth slowdown
				transform.position = Vector3.Lerp(swimmingFrom,swimmingTo, invSq);
			}
		} else { // no goal, drift
			Vector3 driftPos = transform.position +
				Mathf.Cos (Time.time/1.3f) * transform.right * driftX * Time.deltaTime +
					Mathf.Cos (Time.time/4.1f) * transform.up * driftY * Time.deltaTime;
			transform.position = SeaBounds.instance.constrain( driftPos );
		} // end of else/drift
	} // end of Update()

} // end of class
