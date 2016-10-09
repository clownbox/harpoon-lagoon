using UnityEngine;
using UnityEngine.UI; // for debug interaction-style cycle button
using System;
using System.Collections;
using System.Reflection;

[Serializable]
public class FishermanAnimFrame
{
	public Renderer body;
	public Renderer pole;
}

public class HarpoonThrower : MonoBehaviour {
	public FishermanAnimFrame[] animPoses;
	private int animPoseNow = 0;
	private bool showSpear = true;

	public GameObject ropeStart;

	public GameObject harpoonPrefab;
	public static HarpoonThrower instance;

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

	void Awake() {
		instance = this;
	}

	void Start() {
		showAnim();
	}

	public void AdvanceAnim() {
		if(animPoseNow >= animPoses.Length-1) {
			showSpear = false;
			animPoseNow = animPoses.Length - 1;
		} else {
			animPoseNow++;
		}
		showAnim();
	}

	public void MidAnim() {
		animPoseNow = 1;
		showAnim();
	}

	public void ResetAnim() {
		showSpear = true;
		animPoseNow = 0;
		showAnim();
	}

	void showAnim() {
		for(int i = 0; i < animPoses.Length; i++) {
			animPoses[i].body.enabled = (i == animPoseNow);
			animPoses[i].pole.enabled = (i == animPoseNow && showSpear);
		}
	}

	public void turnToX(float someX) {
		Vector3 whatPt = animPoses[0].body.transform.position;
		whatPt.z = -0.1f;
		whatPt.x = someX;
		for(int i = 0; i < animPoses.Length; i++) {
			animPoses[i].body.transform.LookAt(whatPt);
			animPoses[i].body.transform.Rotate(270.0f, 22.5f, 0.0f);

			animPoses[i].pole.transform.LookAt(whatPt);
			animPoses[i].pole.transform.Rotate(270.0f, 22.5f, 0.0f);
		}
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
				// cycleInteractionText.text = ""+throwInteraction; // debug display
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
				// cycleInteractionText.text = ""+throwNinjaMode; // debug display
			}
		}
	}

	void Update() {
		enforceThrowInteraction(false);
		enforceNinjaThrowMode(false);
		enforceYankInteraction(true);
	}

	IEnumerator AnimThrow() {
		for(int i = 0; i < animPoses.Length; i++) {
			AdvanceAnim();
			yield return new WaitForSeconds(0.06f);
		}
	}

	public void ThrowAt (Vector3 targetPoint) {
		Vector3 throwFrom = transform.position;

		StartCoroutine( AnimThrow() );

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
				HarpoonDrag.throwForce = HarpoonDrag.MAX_FORCE * 1.75f; // 2.0f
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
		} else {
			SharkHurry.instance.retreatIfNotReady();
			FMODUnity.RuntimeManager.PlayOneShot("event:/harpoon_throw");
			FMODUnity.RuntimeManager.PlayOneShot("event:/harpoon_water");
		}
	}
}
