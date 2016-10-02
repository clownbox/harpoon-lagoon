using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextFadeOut : MonoBehaviour {
	Text myText;
	float fadeTime;
	Color textBaseColor;

	void Awake() {
		myText = GetComponent<Text>();
	}

	// Use this for initialization
	void Start () {
		textBaseColor = myText.color;
	}

	public void showMedalMessage(string msg) {
		myText.text = msg;
		fadeTime = 2.5f;
	}

	public void showDay(int forLevel) { // reminder: levels start at zero
		if(MenuStateMachine.instance.tutStep == MenuStateMachine.TUTORIAL_PHASE.NormalPlay) {
			// Debug.Log("problem from: " + name);
			myText.text = "Day " + (forLevel + 1) + " / " +
				(FishSpawnInfinite.instance.fishLevelOption[
					FishSpawnInfinite.instance.whichFishSeq].fishLevelSeq.Count);
		} else {
			myText.text = "";//"Tip "+(int)MenuStateMachine.instance.tutStep;

			ScoreManager.instance.PostTutMessage(
				MenuStateMachine.instance.tutStepLabel()
			);
		}
		fadeTime = 2.0f;
	}
		
	// Update is called once per frame
	void Update () {
		if(fadeTime <= 0.0f) {
			if(myText.enabled) {
				myText.enabled = false;
			}
		} else {
			fadeTime -= Time.deltaTime;
			float calcBrightness = fadeTime * 2.0f;
			textBaseColor.a = Mathf.Min(calcBrightness, 1.0f);
			if(myText.color.a != textBaseColor.a) {
				myText.color = textBaseColor;
			}
			if(myText.enabled == false) {
				myText.enabled = true;
			}
		}
	}
}
