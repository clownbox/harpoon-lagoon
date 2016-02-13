using UnityEngine;
using System.Collections;

public class ClickTouchReportToThrow : MonoBehaviour {
	public HarpoonThrower htScript;
	public static bool throwUponReleaseNotPress;
	public static bool pullbackNotAimDown;
	public static bool fixedLength = false;

	void Update () {
		if(MenuStateMachine.instance.MenuAllowsInput() == false ||
		   MenuStateMachine.instance.MenuBlocksAction()) {
			return;
		}

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit rhInfo;
		if(Physics.Raycast(ray, out rhInfo)) {
			if(rhInfo.collider.gameObject != gameObject) {
				return; // avoids double throw from sky and water mouse detection areas both running same throw code
			}

			if( (throwUponReleaseNotPress == true && Input.GetButtonUp("Fire1")) ||
				(throwUponReleaseNotPress == false && Input.GetButtonDown("Fire1"))) {

				if( (rhInfo.collider.gameObject.layer == LayerMask.NameToLayer("WaterTouch") && pullbackNotAimDown==false) ||
					(rhInfo.collider.gameObject.layer == LayerMask.NameToLayer("SkyTouch") && pullbackNotAimDown==true)) {

					htScript.ThrowAt(rhInfo.point);
				}
			}
			DashedLine.startVertex = Camera.main.WorldToScreenPoint(htScript.transform.position);
			DashedLine.startVertex.x /= Screen.width;
			DashedLine.startVertex.y /= Screen.height;
			DashedLine.startVertex.z = 0.0f;
			Vector3 mousePos = Input.mousePosition;
			Vector3 atPt = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0);

			if(fixedLength) {
				Vector3 ptDelta = atPt - DashedLine.startVertex;
				DashedLine.endVertex = DashedLine.startVertex + ptDelta.normalized * 1.0f;
			} else {
				DashedLine.endVertex = atPt;
			}
		} else {
			DashedLine.startVertex = DashedLine.endVertex = -1000.0f * Vector3.one;
		}
	}
}
