using UnityEngine;
using System.Collections;

public class runLaps : MonoBehaviour {
	public float moveRate = -3.0f;
	public float distOffScreenToAvoidPop = 4.0f;
	private BobInWater biw;

	void Start() {
		biw = GetComponent<BobInWater>();
	}

	void bumpToRight() {
		Vector3 copyVec = transform.position;
		copyVec.x = SeaBounds.instance.right + distOffScreenToAvoidPop;
		if(biw) {
			biw.startPos = copyVec;
		} else {
			transform.position = copyVec;
		}
	}

	void bumpToLeft() {
		Vector3 copyVec = transform.position;
		copyVec.x = SeaBounds.instance.left - distOffScreenToAvoidPop;
		transform.position = copyVec;
		if(biw) {
			biw.startPos = copyVec;
		} else {
			transform.position = copyVec;
		}
	}

	public void Restart() {
		if(moveRate < 0.0f) {
			bumpToRight();
		} else {
			bumpToLeft();
		}
	}

	// Update is called once per frame
	void Update () {
		if(biw) {
			biw.startPos += FishTime.deltaTime * moveRate * Vector3.right;
		} else {
			transform.position += FishTime.deltaTime * moveRate * Vector3.right;
		}

		if(moveRate < 0.0f) {
			if(transform.position.x < SeaBounds.instance.left - distOffScreenToAvoidPop) {
				bumpToRight();
			}
		} else {
			if(transform.position.x > SeaBounds.instance.right + distOffScreenToAvoidPop) {
				bumpToLeft();
			}
		}
	}
}
