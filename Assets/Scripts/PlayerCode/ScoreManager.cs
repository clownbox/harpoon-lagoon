using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour {
	public int startSpears = 12;

	public int extraHarpoonThreshold = 400;
	public bool extraHarpoonEarnedSinceLastThrow = false;

	public static ScoreManager instance;
	public GameObject scorePopPrefab;

	public static int SCORE_PER_EXTRA_FISH_ON_POLE = 50;
	public static int SCORE_PER_EXTRA_FISH_ON_POLE_THIRD = 75; // + the above
	public static int SCORE_PER_EXTRA_FISH_ON_POLE_PAIR = 100;
	public static int SCORE_PER_SERIES = 1000;
	public static int SCORE_PER_TRIPLE = 500;

	public Text harpoonAwardMessage;
	public Text totalScoreText;
	public Text lastThrowScoreText;
	public Text spearsLeftText;

	public Text bronzeGoalText,
				silverGoalText,
				goldGoalText;

	public enum Medal
	{
		Fail,
		Bronze,
		Silver,
		Gold
	};

	public Text nextScoreText;

	public GameObject gameOverPanel;
	public Text endScoreHeader;
	public Text endScoreText;

	public HarpoonDrag lastThrownSpear;

	private int totalScore;
	private int nextScore;
	private int lastThrowScore;
	private int spearsLeft;

	public int spearsOut;
	private int turtleHits = 0;

	public bool waitingForGameOver = false;

	public Canvas uiCanvas;

	public Medal scoreMedalMeasure() {
		if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.B)) {
			// PlayerPrefs.DeleteAll();
			switch(Random.Range(0, 3)) {
			case 0:
				totalScore = FishSpawnInfinite.instance.GoldGoal();
				break;
			case 1:
				totalScore = FishSpawnInfinite.instance.SilverGoal();
				break;
			case 2:
				totalScore = FishSpawnInfinite.instance.BronzeGoal();
				break;
			}
		}

		if(totalScore >= FishSpawnInfinite.instance.GoldGoal()) {
			return Medal.Gold;
		} else if(totalScore >= FishSpawnInfinite.instance.SilverGoal()) {
			return Medal.Silver;
		} else if(totalScore >= FishSpawnInfinite.instance.BronzeGoal()) {
			return Medal.Bronze;
		} else {
			return Medal.Fail;
		}
	}

	public void PostTutMessage(string setTo) {
		harpoonAwardMessage.text = setTo;
	}

	public void ClearSpearText() {
		if(MenuStateMachine.instance==null || MenuStateMachine.instance.notInTut()) {
			harpoonAwardMessage.text = "";//extraHarpoonThreshold + " pt throw to gain harpoon";
		}
	}

	public void WinLevel(Medal winLevel) {
		switch(winLevel) {
		case Medal.Gold:
			totalScore = FishSpawnInfinite.instance.GoldGoal();
			break;
		case Medal.Silver:
			totalScore = FishSpawnInfinite.instance.SilverGoal();
			break;
		case Medal.Bronze:
			totalScore = FishSpawnInfinite.instance.BronzeGoal();
			break;
		}
		FishSpawnInfinite.instance.RemoveAll();
		FishSpawnInfinite.instance.NextLevel();
	}

	public void ResetScore(bool resetDayAlso = true) {
		spearsOut = 0;
		turtleHits = 0;
		spearsLeft = startSpears;
		totalScore = lastThrowScore = nextScore = 0;
		extraHarpoonEarnedSinceLastThrow = false;
		ClearSpearText();
		totalScoreText.text = "0" + "   ";
		nextScoreText.text = "";
		lastThrowScoreText.text = "0";
		UpdateSpearCount();
		//Debug.Log("ResetScore");
		if(resetDayAlso && FishSpawnInfinite.instance) {
			FishSpawnInfinite.instance.ResetDay();
		}

		waitingForGameOver = false;
	}

	public void UpdateSpearCount() {
		if(spearsLeftText == null || MenuStateMachine.instance==null) {
			return;
		}

		if(MenuStateMachine.instance && MenuStateMachine.instance.notInTut()) {
			bronzeGoalText.text = ""+FishSpawnInfinite.instance.BronzeGoal();
			silverGoalText.text = ""+FishSpawnInfinite.instance.SilverGoal();
			goldGoalText.text = ""+FishSpawnInfinite.instance.GoldGoal();
		} else {
			// spearsLeftText.text = "";//"Tutorial Step " + (int)MenuStateMachine.instance.tutStep;
		}

		/*if(MenuStateMachine.instance && MenuStateMachine.instance.notInTut()) {
			spearsLeftText.text = "Harpoon" + (spearsLeft == 1 ? "" : "s") + ": " + spearsLeft;
		} else {
			spearsLeftText.text = "";//"Tutorial Step " + (int)MenuStateMachine.instance.tutStep;
		}*/
	}

	public void EndOfTutorialMessage() {
		StartCoroutine(ShowGameOverSoon());
	}

	public void InstantGameOver() {
		if(gameOverPanel.activeSelf == false) {
			StartCoroutine(ShowGameOverSoon());
		}
	}

	public void ForceGameOver() {
		spearsLeft = 0;
		ShowGameOverIfNeeded();
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
		if(MenuStateMachine.instance.notInTut()) {
			endScoreHeader.text = "going home early";
			endScoreText.text = "your score: "+totalScore + "\nbronze goal: " + 
				FishSpawnInfinite.instance.BronzeGoal();
			FMODUnity.RuntimeManager.PlayOneShot("event:/game_over");
		} else {
			endScoreHeader.text = "end of tutorial";
			endScoreText.text = "you're ready to fish!";
			FMODUnity.RuntimeManager.PlayOneShot("event:/round_end");
		}
	}

	public void SpearStartReturning(GameObject whichSpear)
	{
		// Destroy(whichSpear.transform.parent.gameObject);
		HarpoonDrag hdScript = whichSpear.GetComponent<HarpoonDrag>();
		if(HarpoonThrower.instance.yankInteraction == HarpoonThrower.YANK_INTERACTION.Auto) {
			hdScript.WaitThenRetract();
		} else {
			hdScript.pausingBeforeReturn = true;
		}
	}

	public void SpearReturned(GameObject whichSpear) {
		spearsOut--;
		if(spearsOut <= 0) {
			FishTime.fishPacing = 1.0f; // restore in case previously using FishTime.useBulletTime
		}
		ShowGameOverIfNeeded();
	}

	public void TurtleStrike() {
		turtleHits++;
		if(turtleHits == 5) {
			MenuStateMachine.instance.DidAchivement(MenuStateMachine.ACHIEVEMENT_ENUM.turtleSoup,100.0f);
		}
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
			/* if(MenuStateMachine.instance.notInTut()) {
				spearsLeft--;
			} */
			ClearSpearText();
			UpdateSpearCount();
			return true;
		}
		return false;
	}

	public void ScoreAddMegaPop(FishMoverBasic fmbScored, HarpoonDrag thrownSpear, int scoreAmt) {
		ScorePop(fmbScored.gameObject,thrownSpear,scoreAmt);
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
	
	public void ScorePop(GameObject enemyScored, HarpoonDrag thrownSpear, int scoreAmt = -1) {
		FishMoverBasic fmbScript = enemyScored.GetComponent<FishMoverBasic>();
		Vector3 textPos;
		HarpoonablePassingMonster hpmScript = enemyScored.GetComponent<HarpoonablePassingMonster>();

		if(fmbScript != null) {
			textPos = fmbScript.diedPos;
		} else if(hpmScript != null) {
			textPos = hpmScript.transform.position;
		} else {
			textPos = enemyScored.transform.position;
		}

		int scoreAdded;

		if(fmbScript && fmbScript.stolenByShark) {
			scoreAdded = 0;
		} else {
			scoreAdded = (scoreAmt == -1 ? 
			(fmbScript ? fmbScript.scoreValue : hpmScript.scoreValue ) : 
			scoreAmt);
		}

		nextScore += scoreAdded;
		// nextScoreText.text = "+"+nextScore; //// since only allowing one harpoon at a time can use Last Score as same val

		if(thrownSpear == lastThrownSpear) {
			lastThrowScore += scoreAdded;
			lastThrowScoreText.text = "" + lastThrowScore;
			if(lastThrowScore >= extraHarpoonThreshold &&
			   extraHarpoonEarnedSinceLastThrow == false) {
				FMODUnity.RuntimeManager.PlayOneShot("event:/new_harpoons");
				if(MenuStateMachine.instance.tutStep == MenuStateMachine.TUTORIAL_PHASE.ExtraSpear) {
					MenuStateMachine.instance.NextStep();
				}
				if(spearsLeft == 0) {
					MenuStateMachine.instance.DidAchivement(MenuStateMachine.ACHIEVEMENT_ENUM.savingThrow,100.0f);
				}
				/* spearsLeft++;
				if(MenuStateMachine.instance.notInTut()) {
					harpoonAwardMessage.text = "You earned an extra harpoon!";
				} */
				extraHarpoonEarnedSinceLastThrow = true;
				UpdateSpearCount();
			}
		}

		GameObject scoreGO = GameObject.Instantiate(scorePopPrefab);
		Text textScript = scoreGO.GetComponent<Text>();
		if(fmbScript && fmbScript.stolenByShark) {
			textScript.text = "STOLEN";
		} else {
			textScript.text = (scoreAdded > 0 ? "+" : "") + scoreAdded;
		}

		scoreGO.transform.SetParent(uiCanvas.transform);
		
		scoreGO.transform.localScale = Vector3.one * 0.8f;
		textPos.z = Camera.main.transform.position.z + 0.35f;
		if(scoreAmt != -1) {
			scoreGO.transform.localScale *= 0.7f;
			// textPos.y += 1.5f;
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
