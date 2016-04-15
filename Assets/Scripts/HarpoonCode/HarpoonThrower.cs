using UnityEngine;
using UnityEngine.UI; // for debug interaction-style cycle button
using System.Collections;

public class HarpoonThrower : MonoBehaviour {
	public GameObject ropeStart;

	public GameObject harpoonPrefab;

	public static bool limitOneHarpoonAtTime = true; // (confirmed 9pm Feb 27 via Slack)

	public enum YANK_INTERACTION
	{
		Swipe,
		Tap,
		Auto,
		NotInitializedYet
	};
	public YANK_INTERACTION yankInteraction = YANK_INTERACTION.Auto; 
	YANK_INTERACTION wasYI = YANK_INTERACTION.NotInitializedYet; // to detect change from inspector or outside of class

	public enum THROW_INTERACTION
	{
		Tap,
		Hold,
		PullBack,
		NotInitializedYet
	};
	public THROW_INTERACTION throwInteraction = THROW_INTERACTION.Hold; 
	THROW_INTERACTION wasTI = THROW_INTERACTION.NotInitializedYet; // to detect change from inspector or outside of class

	public enum THROW_NINJAMODE
	{
		Normal,
		FastLine,
		SlowFish,
		NotInitializedYet
	};
	public THROW_NINJAMODE throwNinjaMode = THROW_NINJAMODE.Normal; 
	THROW_NINJAMODE wasNM = THROW_NINJAMODE.NotInitializedYet; // to detect change from inspector or outside of class

	public BoxCollider skyTouch;
	public BoxCollider waterTouch;

	public static HarpoonThrower instance;

	public Text cycleInteractionText;

	void Start() {
		instance = this;
	}

	public void CycleYankInteraction() {
		yankInteraction++;
		if((int)yankInteraction >= (int)YANK_INTERACTION.NotInitializedYet) {
			yankInteraction = (YANK_INTERACTION)0;
		}
		enforceYankInteraction(true);
	}

	void enforceYankInteraction(bool showOnButtonText) {
		if(wasYI != yankInteraction) {
			wasYI = yankInteraction;
			if(showOnButtonText) {
				// cycleInteractionText.text = ""+yankInteraction; // debug display
			}
		}
	}

	void enforceThrowInteraction(bool showOnButtonText) {
		if(wasTI != throwInteraction) {
			wasTI = throwInteraction;
			DashedLine.enableHoldLine = (throwInteraction != THROW_INTERACTION.Tap);
			ClickTouchReportToThrow.fixedLength = (throwInteraction != THROW_INTERACTION.PullBack);
			ClickTouchReportToThrow.throwUponReleaseNotPress = (throwInteraction != THROW_INTERACTION.Tap);
			ClickTouchReportToThrow.pullbackNotAimDown = (throwInteraction == THROW_INTERACTION.PullBack);

			skyTouch.enabled = (throwInteraction == THROW_INTERACTION.PullBack);
			waterTouch.enabled = !skyTouch.enabled;

			if(showOnButtonText) {
				cycleInteractionText.text = ""+throwInteraction; // debug display
			}
		}
	}

	public void CycleNinjaMode() {
		throwNinjaMode++;
		if((int)throwNinjaMode >= (int)THROW_NINJAMODE.SlowFish) { // currently skipping slow fish mode, rope not supported
			throwNinjaMode = (THROW_NINJAMODE)0;
		}
		enforceNinjaThrowMode(true);
	}

	void enforceNinjaThrowMode(bool showOnButtonText) {
		if(wasNM != throwNinjaMode) {
			wasNM = throwNinjaMode;

			HarpoonDrag.fishTorquesSpear = (throwNinjaMode != THROW_NINJAMODE.FastLine);
			FishTime.useBulletTime = (throwNinjaMode == THROW_NINJAMODE.SlowFish);

			if(showOnButtonText) {
				cycleInteractionText.text = ""+throwNinjaMode; // debug display
			}
		}
	}

	void Update() {
		enforceThrowInteraction(false);
		enforceNinjaThrowMode(false);
		enforceYankInteraction(true);
	}

	public void ThrowAt (Vector3 targetPoint) {
		Vector3 throwFrom = transform.position;

		if(limitOneHarpoonAtTime) {
			if(ScoreManager.instance.spearsOut > 0) { // no throw until prev throw returns
				return;
			}
		}

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
			if(throwNinjaMode == THROW_NINJAMODE.FastLine) {
				HarpoonDrag.throwForce = HarpoonDrag.MAX_FORCE * 2.0f;
			} else {
				HarpoonDrag.throwForce = HarpoonDrag.MAX_FORCE;
			}
		}

		throwFrom.z = SeaBounds.instance.fishLayerZ;
		targetPoint.z = throwFrom.z;

		Quaternion throwRot = Quaternion.LookRotation(targetPoint-throwFrom,Vector3.up);
		throwRot *= Quaternion.AngleAxis(90.0f,Vector3.right);

		GameObject ropeBoth = (GameObject)GameObject.Instantiate(ropeStart,throwFrom,throwRot);
		GameObject ropeSource = ropeBoth.transform.Find("Rope Start").gameObject;
		ropeSource.name = "rope_"+CurrentHarpoonID.harpoonID;

		ropeSource.transform.position = transform.position - Vector3.forward * 2.0f;

		GameObject ropeDest = GameObject.Find("Rope End");
		ropeDest.name = "Rope Live End";

		GameObject harpoonGO = (GameObject)GameObject.Instantiate(harpoonPrefab,throwFrom,throwRot);
		TrackObj trackingScript = ropeDest.GetComponent<TrackObj>();

		trackingScript.matchLoc = harpoonGO.transform.GetChild(0).Find("HarpoonRopeEnd");

		HarpoonDrag hdScript = harpoonGO.GetComponentInChildren<HarpoonDrag>();
		hdScript.myRopeSource = ropeSource.transform;

		CurrentHarpoonID.harpoonID++;

		if(ScoreManager.instance.NewSpearThrown(hdScript) == false) {
			Destroy(harpoonGO);
		}
	}
}
