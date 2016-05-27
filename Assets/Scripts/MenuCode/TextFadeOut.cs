using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextFadeOut : MonoBehaviour {
	Text myText;
	float fadeTime;
	Color textBaseColor;

	// Use this for initialization
	void Start () {
		myText = GetComponent<Text>();
		textBaseColor = myText.color;
	}

	public void showDay(int forLevel) { // reminder: levels start at zero
		myText.text = "Day " + (forLevel+1);
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
