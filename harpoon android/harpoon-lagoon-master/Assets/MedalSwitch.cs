using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MedalSwitch : MonoBehaviour {
	public Sprite bronze;
	public Sprite silver;
	public Sprite gold;
	private Image myImg;

	void Start() {
		// SetMedal( ScoreManager.Medal.Fail );
	}

	public void SetMedal( ScoreManager.Medal whichMedal ) {
		if(myImg == null) {
			myImg = GetComponent<Image>();
		}

		myImg.enabled = (whichMedal != ScoreManager.Medal.Fail);

		if(myImg.enabled) {
			switch(whichMedal) {
			case ScoreManager.Medal.Bronze:
				myImg.sprite = bronze;
				break;
			case ScoreManager.Medal.Silver:
				myImg.sprite = silver;
				break;
			case ScoreManager.Medal.Gold:
				myImg.sprite = gold;
				break;
			}
		}
	}
}
