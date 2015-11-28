using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HarpoonDrag : MonoBehaviour {
	public GameObject stackPoint;
	public GameObject stackSlideEnd;
	List<GameObject> fishStack;
	List<Vector3> stackOffsets;

	Vector3 motion;
	bool hitTarget = false;

	// Use this for initialization
	void Start () {
		fishStack = new List<GameObject>();
		stackOffsets = new List<Vector3>();
		GameObject.Destroy(gameObject,12.0f); // self destruct
		motion = transform.up * 16.75f;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position += motion * Time.deltaTime;
		if(hitTarget) {
			transform.Rotate(Time.deltaTime * Mathf.Cos (Time.time*2.13f)*10.0f,
			                 Time.deltaTime * Mathf.Cos (Time.time*1.2f)*10.0f,
			                 Time.deltaTime * Mathf.Cos (Time.time*3.28f)*10.0f);
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
		motion *= 0.96f;
		motion += 0.8f * Vector3.down * Time.fixedDeltaTime;
	}

	void OnTriggerEnter(Collider other) {
		FishMoverBasic fmbScript = other.gameObject.GetComponent<FishMoverBasic>();
		if(fmbScript && fmbScript.IsAlive() && motion.magnitude >= 1.0f) {

			ScoreManager.instance.ScorePop(other.GetComponent<FishMoverBasic>(),
			                               this);

			motion *= 0.9f;
			float wiggleRand = 4.0f;
			transform.Rotate(Random.Range(-wiggleRand,wiggleRand),
			                 Random.Range(-wiggleRand,wiggleRand),
			                 Random.Range(-wiggleRand,wiggleRand));
			fmbScript.Die();
			Vector3 tempVect = fmbScript.transform.position-
								getProjectedPointOnLine(fmbScript.transform.position,
			                                           stackPoint.transform.position, 
			                                           stackSlideEnd.transform.position);
			stackOffsets.Add ( tempVect );
			fishStack.Add(fmbScript.gameObject);
			other.enabled = false;
			other.transform.parent = transform;
			hitTarget = true;
		}
	}
}
