using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour {
	public int startSpears = 12;

	public static ScoreManager instance;
	public GameObject scorePopPrefab;

	public Text totalScoreText;
	public Text lastThrowScoreText;
	public Text spearsLeftText;

	public GameObject gameOverPanel;
	public Text endScoreText;

	public HarpoonDrag lastThrownSpear;

	private int totalScore;
	private int lastThrowScore;
	private int spearsLeft;

	private int spearsOut;

	public Canvas uiCanvas;

	public void ResetScore() {
		spearsOut = 0;
		spearsLeft = startSpears;
		totalScore = lastThrowScore = 0;
		totalScoreText.text = "0"+"   ";
		lastThrowScoreText.text = "0";
		UpdateSpearCount();
	}

	public void UpdateSpearCount() {
		spearsLeftText.text = "Spears: "+spearsLeft;
	}

	public bool ShowGameOverIfNeeded() {
		if(spearsLeft <= 0) {
			if(gameOverPanel.activeSelf == false) {
				MenuStateMachine.instance.AllMenusOffExcept(gameOverPanel);
				endScoreText.text = "congrats you earned " + totalScore;
			}
			return true;
		}
		return false;
	}

	public void SpearOffScreen(GameObject whichSpear) {
		spearsOut--;
		ShowGameOverIfNeeded();
		for(int i = 0; i < whichSpear.transform.childCount; i++) {
			GameObject eachKid = whichSpear.transform.GetChild(i).gameObject;
			RespawnIfSunkBelow risb = eachKid.GetComponent<RespawnIfSunkBelow>();
			if(risb) {
				risb.CountFish();
			}
		}
		Destroy(whichSpear.transform.parent.gameObject);
	}

	public bool NewSpearThrown(HarpoonDrag newOne) {
		if(spearsLeft > 0) {
			spearsOut++;
			lastThrownSpear = newOne;
			lastThrowScoreText.text = "" + lastThrowScore;
			lastThrowScore = 0;
			spearsLeft--;
			UpdateSpearCount();
			return true;
		}
		return false;
	}
	
	public void ScorePop(FishMoverBasic fmbScored, HarpoonDrag thrownSpear) {
		Vector3 textPos = fmbScored.transform.position;
		int scoreAdded = fmbScored.scoreValue;

		totalScore += scoreAdded;
		totalScoreText.text = ""+totalScore+"   ";

		if(thrownSpear == lastThrownSpear) {
			lastThrowScore += scoreAdded;
			lastThrowScoreText.text = ""+lastThrowScore;
		}

		GameObject scoreGO = GameObject.Instantiate(scorePopPrefab);
		Text textScript = scoreGO.GetComponent<Text>();
		textScript.text = "+" + scoreAdded;
		
		scoreGO.transform.SetParent(uiCanvas.transform);
		
		textPos.z = Camera.main.transform.position.z + 0.35f;
		scoreGO.transform.position = textPos;
		scoreGO.transform.localScale = Vector3.one * 0.8f;
	}
	
	void Awake () {
		if(instance) {
			Destroy(instance);
		}
		instance = this;
		ResetScore();
	}
}
