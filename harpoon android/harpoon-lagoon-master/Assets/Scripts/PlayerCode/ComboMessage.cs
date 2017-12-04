using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ComboMessage : MonoBehaviour {
	public static ComboMessage instance;
	float clearTextTime = 0.0f;
	public Text ComboText;

	void Start () {
		instance = this;
	}

	public void NewMessage(string thisText, int pointValue, FishMoverBasic fmbKilled, HarpoonDrag harpoon) {
		ComboText.text = thisText + " +" + pointValue + "pt";
		clearTextTime = Time.time + 3.0f;
		ScoreManager.instance.ScoreAddMegaPop(fmbKilled, harpoon, pointValue);
	}

	void Update () {
		if(ComboText.text.Length > 0 && Time.time > clearTextTime) {
			ComboText.text = "";	
		}
	}
}
