﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour {
	public int startSpears = 12;

	public int extraHarpoonThreshold = 400;
	public bool extraHarpoonEarnedSinceLastThrow = false;

	public static ScoreManager instance;
	public GameObject scorePopPrefab;

	public Text harpoonAwardMessage;
	public Text totalScoreText;
	public Text lastThrowScoreText;
	public Text spearsLeftText;

	public Text nextScoreText;

	public GameObject gameOverPanel;
	public Text endScoreText;

	public HarpoonDrag lastThrownSpear;

	private int totalScore;
	private int nextScore;
	private int lastThrowScore;
	private int spearsLeft;

	public int spearsOut;

	public bool waitingForGameOver = false;

	public Canvas uiCanvas;

	public void ClearSpearText() {
		harpoonAwardMessage.text=extraHarpoonThreshold + " pt throw to gain harpoon";
	}

	public void ResetScore() {
		spearsOut = 0;
		spearsLeft = startSpears;
		totalScore = lastThrowScore = nextScore = 0;
		extraHarpoonEarnedSinceLastThrow = false;
		ClearSpearText();
		totalScoreText.text = "0"+"   ";
		nextScoreText.text = "";
		lastThrowScoreText.text = "0";
		UpdateSpearCount();
		waitingForGameOver = false;
	}

	public void UpdateSpearCount() {
		spearsLeftText.text = "Spears: "+spearsLeft;
	}

	public bool ShowGameOverIfNeeded() {
		if(waitingForGameOver == false) {
			if(spearsLeft <= 0) {
				if(gameOverPanel.activeSelf == false) {
					StartCoroutine(ShowGameOverSoon());
				}
			}
		}
		return waitingForGameOver;
	}

	IEnumerator ShowGameOverSoon() {
		waitingForGameOver = true;
		yield return new WaitForSeconds(0.5f);
		MenuStateMachine.instance.AllMenusOffExcept(gameOverPanel);
		endScoreText.text = "congrats you earned " + totalScore;
	}

	public void SpearStartReturning(GameObject whichSpear)
	{
		// Destroy(whichSpear.transform.parent.gameObject);
		HarpoonDrag hdScript = whichSpear.GetComponent<HarpoonDrag>();
		hdScript.retractRope();
	}

	public void SpearReturned(GameObject whichSpear) {
		spearsOut--;
		if(spearsOut <= 0) {
			FishTime.fishPacing = 1.0f; // restore in case previously using FishTime.useBulletTime
		}
		ShowGameOverIfNeeded();
	}

	public void TallyHookedScore() {
		totalScore += nextScore;
		totalScoreText.text = "" + totalScore + "   ";

		nextScore = 0;
		nextScoreText.text = "";
	}

	public bool NewSpearThrown(HarpoonDrag newOne) {
		if(spearsLeft > 0) {
			FishTime.fishPacing = (FishTime.useBulletTime ? 0.05f : 1.0f);
			spearsOut++;
			lastThrownSpear = newOne;
			lastThrowScoreText.text = "" + lastThrowScore;
			nextScoreText.text = "";
			lastThrowScore = 0;
			extraHarpoonEarnedSinceLastThrow = false;
			spearsLeft--;
			ClearSpearText();
			UpdateSpearCount();
			return true;
		}
		return false;
	}

	public void ScoreAddMegaPop(FishMoverBasic fmbScored, HarpoonDrag thrownSpear, int scoreAmt) {
		ScorePop(fmbScored,thrownSpear,scoreAmt);
		/*Vector3 textPos = fmbScored.transform.position;
		int scoreAdded = fmbScored.scoreValue;

		totalScore += scoreAdded;
		totalScoreText.text = ""+totalScore+"   ";

		if(thrownSpear == lastThrownSpear) {
			lastThrowScore += scoreAdded;
			lastThrowScoreText.text = ""+lastThrowScore;
			if(lastThrowScore >= extraHarpoonThreshold &&
				extraHarpoonEarnedSinceLastThrow == false) {

				spearsLeft++;
				harpoonAwardMessage.text="You earned an extra harpoon!";
				extraHarpoonEarnedSinceLastThrow = true;
				UpdateSpearCount();
			}
		}

		GameObject scoreGO = GameObject.Instantiate(scorePopPrefab);
		Text textScript = scoreGO.GetComponent<Text>();
		textScript.text = "+" + scoreAdded;
		textScript.transform.localScale *= 2.0f;

		scoreGO.transform.SetParent(uiCanvas.transform);

		textPos.z = Camera.main.transform.position.z + 0.35f;
		scoreGO.transform.position = textPos;
		scoreGO.transform.localScale = Vector3.one * 0.8f;*/
	}
	
	public void ScorePop(FishMoverBasic fmbScored, HarpoonDrag thrownSpear, int scoreAmt = -1) {
		Vector3 textPos = fmbScored.transform.position;
		int scoreAdded = (scoreAmt == -1 ? fmbScored.scoreValue : scoreAmt);

		nextScore += scoreAdded;
		// nextScoreText.text = "+"+nextScore; //// since only allowing one harpoon at a time can use Last Score as same val

		if(thrownSpear == lastThrownSpear) {
			lastThrowScore += scoreAdded;
			lastThrowScoreText.text = "" + lastThrowScore;
			if(lastThrowScore >= extraHarpoonThreshold &&
			   extraHarpoonEarnedSinceLastThrow == false) {

				spearsLeft++;
				harpoonAwardMessage.text = "You earned an extra harpoon!";
				extraHarpoonEarnedSinceLastThrow = true;
				UpdateSpearCount();
			}
		}

		GameObject scoreGO = GameObject.Instantiate(scorePopPrefab);
		Text textScript = scoreGO.GetComponent<Text>();
		textScript.text = "+" + scoreAdded;

		scoreGO.transform.SetParent(uiCanvas.transform);
		
		scoreGO.transform.localScale = Vector3.one * 0.8f;
		textPos.z = Camera.main.transform.position.z + 0.35f;
		if(scoreAmt != -1) {
			scoreGO.transform.localScale *= 2.0f;
			textPos.y += 1.5f;
		}
		scoreGO.transform.position = textPos;
	}
	
	void Awake () {
		if(instance) {
			Destroy(instance);
		}
		instance = this;
		ResetScore();
	}
}
