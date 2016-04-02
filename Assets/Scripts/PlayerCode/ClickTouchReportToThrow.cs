using UnityEngine;
using System.Collections;

public class ClickTouchReportToThrow : MonoBehaviour {
	public HarpoonThrower htScript;
	public static bool throwUponReleaseNotPress;
	public static bool pullbackNotAimDown;
	public static bool fixedLength = false;

	public GameObject playerBoat;

	void Update () {
		if(MenuStateMachine.instance.MenuAllowsInput() == false ||
		   MenuStateMachine.instance.MenuBlocksAction()) {
			return;
		}
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit rhInfo;
		if(Physics.Raycast(ray, out rhInfo)) {

			if(FishTime.isMovingBetweenThrows && FishTime.timeState != FishTime.TIME_MODE.RealTime) {
				return;
			}

			if(rhInfo.collider.gameObject != gameObject) {
				return; // avoids double throw from sky and water mouse detection areas both running same throw code
			}

			if(ScoreManager.instance.spearsOut > 0 && HarpoonThrower.limitOneHarpoonAtTime) {
				return;
			}

			DashedLine.startVertex = Camera.main.WorldToScreenPoint(htScript.transform.position);
			DashedLine.startVertex.x /= Screen.width;
			DashedLine.startVertex.y /= Screen.height;
			DashedLine.startVertex.z = 0.0f;
			Vector3 mousePos = Input.mousePosition;
			Vector3 atPt = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0);

			float boatTilt = htScript.transform.rotation.eulerAngles.z;
			if(boatTilt > 180.0f) {
				boatTilt = 360.0f + boatTilt; // turn high positive into negative, so that...
			}
			boatTilt *= 2.5f * Mathf.Deg2Rad; // linear multiplier will scale it correctly

			// translate point back to origin:
			atPt -= DashedLine.startVertex;

			// rotate point
			float s = Mathf.Sin(boatTilt);
			float c = Mathf.Cos(boatTilt);
			float xnew = atPt.x * c - atPt.y * s;
			float ynew = atPt.x * s + atPt.y * c;
			atPt.x = xnew;
			atPt.y = ynew;

			atPt += DashedLine.startVertex;

			if(fixedLength) {
				Vector3 ptDelta = atPt - DashedLine.startVertex;
				DashedLine.endVertex = DashedLine.startVertex + ptDelta.normalized * 1.0f;
			} else {
				DashedLine.endVertex = atPt;
			}

			if( (throwUponReleaseNotPress == true && Input.GetButtonUp("Fire1")) ||
				(throwUponReleaseNotPress == false && Input.GetButtonDown("Fire1"))) {

				if( (rhInfo.collider.gameObject.layer == LayerMask.NameToLayer("WaterTouch") && pullbackNotAimDown==false) ||
					(rhInfo.collider.gameObject.layer == LayerMask.NameToLayer("SkyTouch") && pullbackNotAimDown==true)) {

					Vector3 rotatedTarget = rhInfo.point;
					rotatedTarget -= htScript.transform.position;
					xnew = rotatedTarget.x * c - rotatedTarget.y * s;
					ynew = rotatedTarget.x * s + rotatedTarget.y * c;
					rotatedTarget.x = xnew;
					rotatedTarget.y = ynew;
					rotatedTarget += htScript.transform.position;
					htScript.ThrowAt(rotatedTarget);
				}
			}

		} else {
			DashedLine.startVertex = DashedLine.endVertex = -1000.0f * Vector3.one;
		}
	}
}
