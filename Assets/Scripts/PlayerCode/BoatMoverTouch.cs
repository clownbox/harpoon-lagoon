using UnityEngine;
using System.Collections;

public class BoatMoverTouch : MonoBehaviour {
	public BobInWater playerBoat;
	public Vector3 goToPos;

	void Start() {
		StartCoroutine(LateStart());
	}

	IEnumerator LateStart() {
		yield return new WaitForEndOfFrame();
		goToPos = playerBoat.startPos;
	}

	// Update is called once per frame
	void Update () {

		if(MenuStateMachine.instance.MenuAllowsInput() == false ||
			MenuStateMachine.instance.MenuBlocksAction()) {
			return;
		}

		if(ScoreManager.instance.spearsOut > 0) {
			return;
		}
			
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit rhInfo;
		if(Physics.Raycast(ray, out rhInfo)) {
			if(Input.GetButtonDown("Fire1") && rhInfo.collider.gameObject == gameObject) {
				Vector3 newTouchGetX;
				newTouchGetX.x = rhInfo.point.x;
				newTouchGetX.y = goToPos.y;
				newTouchGetX.z = goToPos.z;
				goToPos = newTouchGetX;
			}
		}
	}

	void FixedUpdate() {
		float kVal = 0.02f;
		playerBoat.startPos = goToPos * kVal + playerBoat.startPos * (1.0f - kVal);
	}

}
