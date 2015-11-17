using UnityEngine;
using System.Collections;

public class HarpoonDrag : MonoBehaviour {
	Vector3 motion;
	bool hitTarget = false;

	// Use this for initialization
	void Start () {
		GameObject.Destroy(gameObject,7.0f); // self destruct

		motion = transform.up * 14.2f;
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
		if(fmbScript && fmbScript.IsAlive() && motion.magnitude >= 1.0f) {
			motion *= 0.8f;
			float wiggleRand = 4.0f;
			transform.Rotate(Random.Range(-wiggleRand,wiggleRand),
			                 Random.Range(-wiggleRand,wiggleRand),
			                 Random.Range(-wiggleRand,wiggleRand));
			fmbScript.Die();
			other.transform.parent = transform;
			hitTarget = true;
		}
	}
}
