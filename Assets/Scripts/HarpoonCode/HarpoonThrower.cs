﻿using UnityEngine;
using UnityEngine.UI; // for debug interaction-style cycle button
using System.Collections;

public class HarpoonThrower : MonoBehaviour {
	public GameObject harpoonPrefab;
	public enum THROW_INTERACTION
	{
		Tap,
		Hold,
		PullBack,
		NotInitializedYet
	};
	public THROW_INTERACTION throwInteraction = THROW_INTERACTION.Tap; 
	THROW_INTERACTION wasTI = THROW_INTERACTION.NotInitializedYet; // to detect change from inspector or outside of class
		
	public BoxCollider skyTouch;
	public BoxCollider waterTouch;

	public Text cycleInteractionText;

	public void CycleInteraction() {
		throwInteraction++;
		if((int)throwInteraction >= (int)THROW_INTERACTION.NotInitializedYet) {
			throwInteraction = (THROW_INTERACTION)0;
		}
		enforceThrowInteraction();
	}

	void enforceThrowInteraction() {
		if(wasTI != throwInteraction) {
			wasTI = throwInteraction;
			DashedLine.enableHoldLine = (throwInteraction != THROW_INTERACTION.Tap);
			ClickTouchReportToThrow.fixedLength = (throwInteraction != THROW_INTERACTION.PullBack);
			ClickTouchReportToThrow.throwUponReleaseNotPress = (throwInteraction != THROW_INTERACTION.Tap);
			ClickTouchReportToThrow.pullbackNotAimDown = (throwInteraction == THROW_INTERACTION.PullBack);

			skyTouch.enabled = (throwInteraction == THROW_INTERACTION.PullBack);
			waterTouch.enabled = !skyTouch.enabled;

			cycleInteractionText.text = ""+throwInteraction; // debug display
		}
	}

	void Update() {
		enforceThrowInteraction();
	}

	public void ThrowAt (Vector3 targetPoint) {
		Vector3 throwFrom = transform.position;

		if(throwInteraction == THROW_INTERACTION.PullBack) {
			Vector3 ptDiff = targetPoint - throwFrom;
			ptDiff.z = 0.0f;

			targetPoint = throwFrom - 2 * ptDiff; // target point across the axis from finger

			// vary force as well
			float bowPull;
			bowPull = (ptDiff).magnitude / 2.0f;
			bowPull = Mathf.Clamp(bowPull, 0.4f, 1.2f);
			Debug.Log(bowPull);

			HarpoonDrag.throwForce = HarpoonDrag.MAX_FORCE * bowPull;
		} else {
			HarpoonDrag.throwForce = HarpoonDrag.MAX_FORCE;
		}

		throwFrom.z = SeaBounds.instance.fishLayerZ;
		targetPoint.z = throwFrom.z;

		Quaternion throwRot = Quaternion.LookRotation(targetPoint-throwFrom,Vector3.up);
		throwRot *= Quaternion.AngleAxis(90.0f,Vector3.right);

		GameObject harpoonGO = (GameObject)GameObject.Instantiate(harpoonPrefab,throwFrom,throwRot);
		HarpoonDrag hdScript = harpoonGO.GetComponentInChildren<HarpoonDrag>();
		if(ScoreManager.instance.NewSpearThrown(hdScript) == false) {
			Destroy(harpoonGO);
		}
	}
}
