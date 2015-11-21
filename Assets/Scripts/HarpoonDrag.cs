using UnityEngine;
using System.Collections;

public class HarpoonDrag : MonoBehaviour {
	Vector3 motion;
	bool hitTarget = false;
	CapsuleCollider myColl;
	Vector3 tempCollPos = Vector3.zero;

	// Use this for initialization
	void Start () {
		GameObject.Destroy(gameObject,12.0f); // self destruct
		myColl = GetComponentInChildren<CapsuleCollider>();
		tempCollPos.y = Random.Range(-0.35f,-0.25f);
		myColl.center = tempCollPos;
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
	}

	void FixedUpdate() {
		motion *= 0.96f;
		motion += 0.8f * Vector3.down * Time.fixedDeltaTime;
	}

	void OnTriggerEnter(Collider other) {
		FishMoverBasic fmbScript = other.gameObject.GetComponent<FishMoverBasic>();
		if(fmbScript && fmbScript.IsAlive() && motion.magnitude >= 1.0f &&
		   tempCollPos.y < 0.55f) {

			ScoreManager.instance.ScorePop(other.GetComponent<FishMoverBasic>(),
			                               this);

			motion *= 0.9f;
			float wiggleRand = 4.0f;
			transform.Rotate(Random.Range(-wiggleRand,wiggleRand),
			                 Random.Range(-wiggleRand,wiggleRand),
			                 Random.Range(-wiggleRand,wiggleRand));
			fmbScript.Die();
			tempCollPos.y += Random.Range(0.25f,0.35f);
			myColl.center = tempCollPos;
			other.enabled = false;
			other.transform.parent = transform;
			hitTarget = true;
		}
	}
}
