using UnityEngine;
using System.Collections;

public class BobInWater : MonoBehaviour {
	public float dampen = 1.0f;

	Vector3 startPos;

	void Start() {
		startPos = transform.position;
	}
	// Update is called once per frame
	void Update () {
		transform.rotation = Quaternion.AngleAxis((Mathf.Cos (FishTime.time) * 1.2f +
			Mathf.Cos (FishTime.time*1.7f) * 1.2f)*
		                                          dampen,
		                                          Vector3.forward);
		transform.position = startPos + 
			(Vector3.right * Mathf.Cos (FishTime.time/3.7f) * 0.1f +
				Vector3.up * Mathf.Cos (FishTime.time*0.3f) * 0.05f)*dampen;
	}
}
