using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HarpoonDrag : MonoBehaviour {
	public GameObject stackPoint;
	public GameObject stackSlideEnd;
	List<GameObject> fishStack;
	List<Vector3> stackOffsets;

	public Transform myRopeSource;
	public Vector3 myRopeSourceStartPos; // changed while retracting

	public static float RETRACT_SPEED = 15.0f;
	public static float RETRACTED_CLOSE_ENOUGH_TO_VANISH = 0.2f;
	public static int MAX_FISH_PER_HARPOON = 3;
	public static int MAX_FISH_TO_SCORE = 3;
	public static float MIN_KILL_MOTION = 1.0f;

	public static float MAX_FORCE = 16.75f;
	public static float throwForce;

	public static bool fishTorquesSpear = true;
	public bool pausingBeforeReturn = false;
	public bool isRopeReturning = false;
	private bool freezeAfterKillDontSink = true; // requested on Mar 4 on slack
	private float swipeTouchDownY = 0.0f;

	public Vector3 motion;
	bool hitTarget = false;
	private bool forceStop = false;

	// Use this for initialization
	void Start () {
		fishStack = new List<GameObject>();
		stackOffsets = new List<Vector3>();

		motion = transform.up * throwForce;
		myRopeSourceStartPos = myRopeSource.position;
	}
		
	// Update is called once per frame
	void Update () {
		if(MenuStateMachine.instance.MenuBlocksAction()) {
			return;
		}

		if(isRopeReturning) {

			if(myRopeSource == null) {
				Debug.Log("myRopeSource missing on harpoon");
				return;
			}

			Vector3 diffBack = myRopeSourceStartPos - transform.position;
			transform.position += diffBack.normalized * Time.deltaTime * RETRACT_SPEED;
			// by pulling the whole rope back it doesn't look like the harpoon is self propelled
			myRopeSource.parent.position += diffBack.normalized * Time.deltaTime * RETRACT_SPEED;

			if(diffBack.magnitude < RETRACTED_CLOSE_ENOUGH_TO_VANISH || transform.position.y > myRopeSourceStartPos.y) {
				ScoreManager.instance.TallyHookedScore();

				for(int i = 0; i < transform.childCount; i++) {
					GameObject eachKid = transform.GetChild(i).gameObject;
					RespawnIfSunkBelow risb = eachKid.GetComponent<RespawnIfSunkBelow>();
					if(risb) {
						risb.CountFish();
					}
				}

				ScoreManager.instance.SpearReturned(gameObject);

				Destroy(gameObject);
				Destroy(myRopeSource.parent.gameObject);
				string harpoonIDToClear = myRopeSource.name.Replace("rope_", "");
				string tubeRendToClear = "TubeRenderer_" + harpoonIDToClear;

				// Debug.Log(tubeRendToClear);
				GameObject ropeRendToRemove = GameObject.Find(tubeRendToClear);
				Destroy(ropeRendToRemove);
			}
			return;
		}

		if(freezeAfterKillDontSink && pausingBeforeReturn) {
			transform.position += Mathf.Cos(Time.time * 1.5f) * 0.15f * Time.deltaTime * Vector3.up;
		} else {
			transform.position += motion * Time.deltaTime;
		}

		fishPoleSlide();

		if(pausingBeforeReturn) {
			if(HarpoonThrower.instance.yankInteraction != HarpoonThrower.YANK_INTERACTION.Auto) {

				if(Input.GetMouseButtonDown(0)) {
					if(HarpoonThrower.instance.yankInteraction == HarpoonThrower.YANK_INTERACTION.Tap) {
						retractRope();
					} else if(HarpoonThrower.instance.yankInteraction == HarpoonThrower.YANK_INTERACTION.Swipe) {
						swipeTouchDownY = Input.mousePosition.y;
					}
				}

				if(HarpoonThrower.instance.yankInteraction == HarpoonThrower.YANK_INTERACTION.Swipe) {
					if(Input.GetMouseButtonUp(0)) {
						if(Input.mousePosition.y > swipeTouchDownY) {
							retractRope();
						}
					}
				}
			}

			return;
		}

		if(hitTarget && fishTorquesSpear) {
			transform.Rotate(Time.deltaTime * Mathf.Cos (Time.time*2.13f)*10.0f,
			                 Time.deltaTime * Mathf.Cos (Time.time*1.2f)*10.0f,
			                 Time.deltaTime * Mathf.Cos (Time.time*3.28f)*10.0f);
		}

		if(motion.magnitude < MIN_KILL_MOTION) {
			if(HarpoonThrower.instance.yankInteraction == HarpoonThrower.YANK_INTERACTION.Auto) {
				StartCoroutine(DelayThenRetract());
			} else {
				pausingBeforeReturn = true;
			}
			if(ScoreManager.instance.spearsOut <= 1) {
				FishTime.fishPacing = 1.0f; // restore in case previously using FishTime.useBulletTime
			}
		}
	}

	public void fishPoleSlide() {
		float stackSpot = 0.0f;
		float slideK = 0.9f;
		Vector3 slideTo;
		float packingAmt = 0.33f;

		if(fishStack.Count > 3) {
			packingAmt = 1.0f/fishStack.Count;
		}

		for(int i=0;i<fishStack.Count;i++) {
			GameObject eachFish = fishStack[i];
			if(eachFish == null) { // broken reference, assume game has been reset, bail out!
				Destroy(gameObject);
				return;
			}
			Vector3 fishOffset = stackOffsets[i];
			slideTo = stackSpot * stackPoint.transform.position +
				(1.0f-stackSpot) * stackSlideEnd.transform.position;
			
			slideTo += fishOffset;
			
			eachFish.transform.position = slideK * eachFish.transform.position +
				(1.0f-slideK) * slideTo;
			
			stackSpot += packingAmt;
		}
	}

	public static Vector3 getProjectedPointOnLine(Vector3 p, Vector3 v1, Vector3 v2)
	{
		Vector2 e1 = new Vector2(v2.x - v1.x, v2.y - v1.y);
		Vector2 e2 = new Vector2(p.x - v1.x, p.y - v1.y);
		float valDp = Vector2.Dot(e1, e2);

		float len2 = e1.x * e1.x + e1.y * e1.y;

		return new Vector3( v1.x + (valDp * e1.x) / len2,
		                   v1.y + (valDp * e1.y) / len2,
		                    p.z);
	}

	void FixedUpdate() {
		if(MenuStateMachine.instance.MenuBlocksAction()) {
			return;
		}

		if(pausingBeforeReturn && freezeAfterKillDontSink) {
			motion = Vector3.zero;
		} else {
			motion *= 0.96f;
			motion += 0.8f * Vector3.down * Time.fixedDeltaTime;
		}
	}

	public void retractRope() {
		isRopeReturning = true;
	}

	void OnTriggerEnter(Collider other) {
		if(pausingBeforeReturn || isRopeReturning) {
			return;
		}

		HarpoonablePassingMonster hpmScript = other.gameObject.GetComponentInParent<HarpoonablePassingMonster>();
		if(hpmScript && hpmScript.IsAlive() && motion.magnitude >= MIN_KILL_MOTION &&
		   fishStack.Count < MAX_FISH_PER_HARPOON) { // limiting to 3 fish on pole at a time

			if(hpmScript.scoreValue != 0) {
				ScoreManager.instance.ScorePop(hpmScript.gameObject,
					this, -1);
			}

			if(hpmScript.stopsHarpoon) {
				forceStop = true;
				FMODUnity.RuntimeManager.PlayOneShot("event:/wrong_fish");

				if(MenuStateMachine.instance.tutStep == MenuStateMachine.TUTORIAL_PHASE.Monsters) {
					MenuStateMachine.instance.NextStep();
				}
			} else {
				FMODUnity.RuntimeManager.PlayOneShot("event:/fish_hit");
				FMODUnity.RuntimeManager.PlayOneShot("event:/fish_dead");
			}
			
			hpmScript.Die();
		}

		FishMoverBasic fmbScript = other.gameObject.GetComponentInParent<FishMoverBasic>();
		if(fmbScript && fmbScript.IsAlive() && motion.magnitude >= MIN_KILL_MOTION &&
			fishStack.Count < MAX_FISH_PER_HARPOON) { // limiting to 3 fish on pole at a time

			ScoreManager.instance.ScorePop(fmbScript.gameObject,
			                               this);

			if(fishTorquesSpear) {
				motion *= 0.9f;
				float wiggleRand = 4.0f;
				transform.Rotate(Random.Range(-wiggleRand,wiggleRand),
				                 Random.Range(-wiggleRand,wiggleRand),
				                 Random.Range(-wiggleRand,wiggleRand));
			}
			fmbScript.Die();
			Vector3 tempVect = fmbScript.transform.position-
								getProjectedPointOnLine(fmbScript.transform.position,
			                                           stackPoint.transform.position, 
			                                           stackSlideEnd.transform.position);
			stackOffsets.Add ( tempVect );
			fishStack.Add(fmbScript.gameObject);

			if(MenuStateMachine.instance.tutStep == MenuStateMachine.TUTORIAL_PHASE.SpearFish) {
				MenuStateMachine.instance.NextStep();
			}

			FMODUnity.RuntimeManager.PlayOneShot("event:/fish_hit");
			FMODUnity.RuntimeManager.PlayOneShot("event:/fish_dead");

			if(fishStack.Count >= MAX_FISH_TO_SCORE) {
				Debug.Log(fishStack.Count);
				if(MenuStateMachine.instance.tutStep == MenuStateMachine.TUTORIAL_PHASE.SpearThree) {
					MenuStateMachine.instance.NextStep();
				}

				// look at the string passed to NewMessage() for indication of which rule it's testing for

				float scaleWas = 0.0f;
				bool rulePassed = true;
				int sizeChanges = 0;
				int MIN_DIFF_SIZES = 3; // to rule out 3 of a kind AAA or 2 of a kind AABB
				for(int i = 0; i < fishStack.Count; i++) {
					if(fishStack[i].transform.localScale.x >= scaleWas) {
						if(fishStack[i].transform.localScale.x > scaleWas) {
							sizeChanges++;
						}
						scaleWas = fishStack[i].transform.localScale.x;
					} else {
						rulePassed = false;
						break;
					}
				}
				if(rulePassed && sizeChanges>=MIN_DIFF_SIZES) {
					ComboMessage.instance.NewMessage(fishStack.Count+" small to big!", 1000, fmbScript, this);
				}


				scaleWas = 1000.0f;
				rulePassed = true;
				sizeChanges = 0;
				for(int i = 0; i < fishStack.Count; i++) {
					if(fishStack[i].transform.localScale.x <= scaleWas) {
						if(fishStack[i].transform.localScale.x < scaleWas) {
							sizeChanges++;
						}
						scaleWas = fishStack[i].transform.localScale.x;
					} else {
						rulePassed = false;
						break;
					}
				}
				if(rulePassed && sizeChanges>=MIN_DIFF_SIZES) {
					ComboMessage.instance.NewMessage(fishStack.Count+" big to small!", 350, fmbScript, this);
				}

				FishMoverBasic.FishBreed fishType = fishStack[0].GetComponent<FishMoverBasic>().myKind;
				rulePassed = true;
				for(int i = 1; i < fishStack.Count; i++) { // skipping 1st since already checked [0] to set example
					if(fishType != fishStack[i].GetComponent<FishMoverBasic>().myKind) {
						rulePassed = false;
						break;
					}
				}
				if(rulePassed) {
					ComboMessage.instance.NewMessage(fishStack.Count+" all same!", 650, fmbScript, this);
				}
				// OK: AAAAA
				// NOT OK: AAABB
			}

			other.enabled = false;
			other.transform.parent.parent = transform;
			hitTarget = true;
		}
		if(fishStack.Count >= MAX_FISH_PER_HARPOON || forceStop) {
			motion = Vector3.zero; // snap to halt
			if(HarpoonThrower.instance.yankInteraction == HarpoonThrower.YANK_INTERACTION.Auto) {
				WaitThenRetract();
			} else {
				pausingBeforeReturn = true;
			}
		}
	}

	public void WaitThenRetract() {
		StartCoroutine(DelayThenRetract());
	}

	IEnumerator DelayThenRetract() {
		pausingBeforeReturn = true;

		HarpoonThrower.instance.MidAnim();

		yield return new WaitForSeconds(0.75f);
		FMODUnity.RuntimeManager.PlayOneShot("event:/harpoon_retrieve");
		isRopeReturning = true;

		if(MenuStateMachine.instance.tutStep == MenuStateMachine.TUTORIAL_PHASE.ReleaseToThrow) {
			MenuStateMachine.instance.NextStep();
		}

		HarpoonThrower.instance.ResetAnim();
	}

}
