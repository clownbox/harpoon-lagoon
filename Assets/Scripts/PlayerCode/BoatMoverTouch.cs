using UnityEngine;
using System.Collections;

public class BoatMoverTouch : MonoBehaviour {
	public BobInWater playerBoat;
	public Vector3 goToPos;
	float moveLoopVolTarget = 0.0f;
	float moveLoopVolNow = 0.0f;
	private FMOD.Studio.EventInstance moveLoopEvt;

	void Start() {
		moveLoopEvt = FMODUnity.RuntimeManager.CreateInstance("event:/ship_move_loop");
		moveLoopEvt.setVolume(0.0f);
		moveLoopEvt.start();
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

				FMODUnity.RuntimeManager.PlayOneShot("event:/ship_move");
				moveLoopVolTarget = 1.0f;

				if(MenuStateMachine.instance.tutStep == MenuStateMachine.TUTORIAL_PHASE.SteerBoat) {
					MenuStateMachine.instance.NextStep();
				}
			}
		}
	}

	void FixedUpdate() {
		float kVal = 0.02f;
		playerBoat.startPos = goToPos * kVal + playerBoat.startPos * (1.0f - kVal);

		if(Vector3.Distance(playerBoat.startPos, goToPos) < 0.5f) {
			moveLoopVolTarget = 0.0f;
		}
		Debug.Log(moveLoopVolNow);

		float soundKVal = 0.06f;
		moveLoopVolNow = moveLoopVolTarget * soundKVal + moveLoopVolNow * (1.0f - soundKVal);
		moveLoopEvt.setVolume(moveLoopVolNow);
	}

}
