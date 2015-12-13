using UnityEngine;
using System.Collections;

public class runLaps : MonoBehaviour {
	public float moveRate = -3.0f;
	public float distOffScreenToAvoidPop = 4.0f;

	void Start() {
	}

	// Update is called once per frame
	void Update () {
		transform.position += Time.deltaTime * moveRate * Vector3.right;
		if(transform.position.x < SeaBounds.instance.left - distOffScreenToAvoidPop) {
			Vector3 copyVec = transform.position;
			copyVec.x = SeaBounds.instance.right + distOffScreenToAvoidPop;
			transform.position = copyVec;
		}
	}
}
