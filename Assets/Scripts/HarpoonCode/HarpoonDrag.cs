using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HarpoonDrag : MonoBehaviour {
	public GameObject stackPoint;
	public GameObject stackSlideEnd;
	List<GameObject> fishStack;
	List<Vector3> stackOffsets;

	public static float MAX_FORCE = 16.75f;
	public static float throwForce;

	public static bool fishTorquesSpear = true;

	Vector3 motion;
	bool hitTarget = false;

	// Use this for initialization
	void Start () {
		fishStack = new List<GameObject>();
		stackOffsets = new List<Vector3>();
		GameObject.Destroy(gameObject,50.0f); // self destruct in case it somehow misses edge boundary triggers

		motion = transform.up * throwForce;
	}
		
	// Update is called once per frame
	void Update () {
		if(MenuStateMachine.instance.MenuBlocksAction()) {
			return;
		}

		transform.position += motion * Time.deltaTime;
		if(hitTarget && fishTorquesSpear) {
			transform.Rotate(Time.deltaTime * Mathf.Cos (Time.time*2.13f)*10.0f,
			                 Time.deltaTime * Mathf.Cos (Time.time*1.2f)*10.0f,
			                 Time.deltaTime * Mathf.Cos (Time.time*3.28f)*10.0f);
		}

		if(motion.magnitude < 1.0f && ScoreManager.instance.spearsOut <= 1) {
			FishTime.fishPacing = 1.0f; // restore in case previously using FishTime.useBulletTime
		}

		fishPoleSlide();
	}

	public void fishPoleSlide() {
		float stackSpot = 0.0f;
		float slideK = 0.95f;
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

	public Vector3 getProjectedPointOnLine(Vector3 p, Vector3 v1, Vector3 v2)
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

		motion *= 0.96f;
		motion += 0.8f * Vector3.down * Time.fixedDeltaTime;
	}

	void OnTriggerEnter(Collider other) {
		FishMoverBasic fmbScript = other.gameObject.GetComponent<FishMoverBasic>();
		if(fmbScript && fmbScript.IsAlive() && motion.magnitude >= 1.0f &&
			fishStack.Count < 3) { // limiting to 3 fish on pole at a time

			ScoreManager.instance.ScorePop(fmbScript,
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

			if(fishStack.Count >= 3) {
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
				// OK: 11223
				// NOT OK: 1231


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
				// OK: 33211
				// NOT OK: 2321

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
			other.transform.parent = transform;
			hitTarget = true;
		}
	}
}
