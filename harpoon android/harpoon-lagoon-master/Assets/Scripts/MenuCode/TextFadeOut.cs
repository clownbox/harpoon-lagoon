using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextFadeOut : MonoBehaviour {
	public Text myTextFG;
	public Text myTextBG;
	public Image mainPanel;

	public Image medalImg;
	public Sprite[] medalKinds;

	Color fadeColor = Color.white;
	Color fadeShadowColor = Color.black;

	float fadeTime = 0.0f;

	public void showMedalMessage(string msg) {
		myTextFG.text = myTextBG.text = msg + " Medal";
		switch(msg) {
			case "Bronze":
				medalImg.sprite = medalKinds[0];
				break;
			case "Silver":
				medalImg.sprite = medalKinds[1];
				break;
			case "Gold":
				medalImg.sprite = medalKinds[2];
				break;
		}
		fadeTime = 2.5f;
	}

	public void showDay(int forLevel) { // reminder: levels start at zero
		if(MenuStateMachine.instance.tutStep == MenuStateMachine.TUTORIAL_PHASE.NormalPlay) {
			// Debug.Log("problem from: " + name);
			myTextFG.text = myTextBG.text = "Day " + (forLevel + 1) + " / " +
				(FishSpawnInfinite.instance.fishLevelOption[
					FishSpawnInfinite.instance.whichFishSeq].fishLevelSeq.Count);
		} else {
			myTextFG.text = myTextBG.text = "";//"Tip "+(int)MenuStateMachine.instance.tutStep;

			ScoreManager.instance.PostTutMessage(
				MenuStateMachine.instance.tutStepLabel()
			);
		}
		fadeTime = 2.0f;
	}
		
	// Update is called once per frame
	void Update () {
		if(fadeTime <= 0.0f) {
			if(myTextFG.enabled) {
				mainPanel.enabled = myTextBG.enabled = myTextFG.enabled = false;
				if(medalImg) {
					medalImg.enabled = false;
				}
			}
		} else {
			fadeTime -= Time.deltaTime;
			float calcBrightness = fadeTime * 2.0f;
			fadeShadowColor.a = fadeColor.a = Mathf.Min(calcBrightness, 1.0f);
			if(myTextFG.color.a != fadeColor.a) {
				mainPanel.color = myTextFG.color = fadeColor;
				if(medalImg) {
					medalImg.color = fadeColor;
				}
				myTextBG.color = fadeShadowColor;
			}
			if(myTextFG.enabled == false) {
				mainPanel.enabled = myTextBG.enabled = myTextFG.enabled = true;
				if(medalImg) {
					medalImg.enabled = true;
				}
			}
		}
	}
}
