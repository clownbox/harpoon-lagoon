using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogoFadeAndAdvance : MonoBehaviour {
	Image clownLogo;
	Color fadeLevel = Color.white;
	enum fading{fadeBefore,fadeIn,fadeHold,fadeOut};
	fading fadingStage = fading.fadeBefore;
	float beforeFadeTime = 0.6f;
	float fadingTimeScale = 0.55f;
	float fadeWaitTime = 1.6f;

	// Use this for initialization
	void Start () {
		clownLogo = GetComponent<Image>();
		fadeLevel.a = 0.0f;
		clownLogo.color = fadeLevel;
	}
	
	// Update is called once per frame
	void Update () {
		switch(fadingStage) {
		case fading.fadeBefore:
			beforeFadeTime -= Time.deltaTime;
			if(beforeFadeTime < 0.0f) {
				fadingStage++;
			}
			break;
		case fading.fadeIn:
			fadeLevel.a += Time.deltaTime*fadingTimeScale;
			if(fadeLevel.a >= 1.0f) {
				fadingStage++;
				fadeLevel.a = 1.0f;
			}
			break;
		case fading.fadeHold:
			fadeWaitTime -= Time.deltaTime;
			if(fadeWaitTime < 0.0f) {
				fadingStage++;
			}
			break;
		case fading.fadeOut:
			fadeLevel.a -= Time.deltaTime*fadingTimeScale;
			if(fadeLevel.a <= 0.0f) {
				fadeLevel.a = 0.0f;
				SceneManager.LoadScene("Main");
			}
			break;
		}
		clownLogo.color = fadeLevel;
	}
}
