using UnityEngine;
using System.Collections;

public class FishTime : MonoBehaviour {
	public static float fishPacing = 1.0f;
	public static bool useBulletTime = false;

	public static float deltaTime;
	public static float time;
	public static float fixedDeltaTime;
	void Start() {
		time = Time.time;
	}

	void Update() {
		fixedDeltaTime = fishPacing * Time.fixedDeltaTime;
		deltaTime = fishPacing * Time.deltaTime;
		time += deltaTime;
	}
}
