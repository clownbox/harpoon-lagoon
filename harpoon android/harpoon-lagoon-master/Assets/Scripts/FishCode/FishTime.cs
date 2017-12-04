using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FishTime : MonoBehaviour {
	public float betweenTurnsMoveSpeedMult = 5.0f;
	public float moveTimeBetweenThrows = 2.85f;
	public static float fishPacing = 1.0f;
	public static bool useBulletTime = false;
	public static bool isMovingBetweenThrows = false;

	public static float deltaTime;
	public static float time;
	public static float fixedDeltaTime;
	float startOrthoSize;
	float pullBackOrthoMult = 1.12f;

	int wasSpearsOut;

	public Text cycleModeText;

	public enum TIME_MODE
	{
		RealTime,
		Freeze,
		VerySlow,
		NotInitializedYet
	};

	public static TIME_MODE timeState = TIME_MODE.RealTime; 

	public void CycleInteraction() {
		timeState++;
		if((int)timeState >= (int)TIME_MODE.NotInitializedYet) {
			timeState = (TIME_MODE)0;
		}
		showTimeMode();
	}

	void showTimeMode() {
		// cycleModeText.text = ""+timeState;
	}

	void Start() {
		showTimeMode();
		startOrthoSize = Camera.main.orthographicSize;
		time = Time.time;
		StartCoroutine(BetweenThrowsDelay());
	}

	IEnumerator BetweenThrowsDelay() {
		isMovingBetweenThrows = true;
		yield return new WaitForSeconds(moveTimeBetweenThrows);
		isMovingBetweenThrows = false;
	}

	void Update() {

		if(ScoreManager.instance.spearsOut < wasSpearsOut) {
			StartCoroutine(BetweenThrowsDelay());
		}
		wasSpearsOut = ScoreManager.instance.spearsOut;

		if(isMovingBetweenThrows && timeState != TIME_MODE.RealTime) {
			fixedDeltaTime = betweenTurnsMoveSpeedMult * fishPacing * Time.fixedDeltaTime;
			deltaTime = betweenTurnsMoveSpeedMult * fishPacing * Time.deltaTime;
		} else switch(timeState) {
		case TIME_MODE.RealTime:
			fixedDeltaTime = fishPacing * Time.fixedDeltaTime;
			deltaTime = fishPacing * Time.deltaTime;
			break;
		case TIME_MODE.Freeze:
			fixedDeltaTime = 0.0f;
			deltaTime = 0.0f;
			break;
		case TIME_MODE.VerySlow:
			fixedDeltaTime = 0.1f * fishPacing * Time.fixedDeltaTime;
			deltaTime = 0.1f * fishPacing * Time.deltaTime;
			break;
		}
		time += deltaTime;
	}

	void FixedUpdate() {
		if(timeState == TIME_MODE.RealTime || MenuStateMachine.instance.MenuBlocksAction()) {
			Camera.main.orthographicSize = startOrthoSize;
		} else {
			float targetSize = (isMovingBetweenThrows ? startOrthoSize * pullBackOrthoMult :
			startOrthoSize);
			float kVal = 0.055f;
			Camera.main.orthographicSize = Camera.main.orthographicSize * (1.0f - kVal) +
			targetSize * (kVal);
		}
	}
}
