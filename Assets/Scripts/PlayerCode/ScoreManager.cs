using UnityEngine;
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

	public void PostTutMessage(string setTo) {
		harpoonAwardMessage.text = setTo;
	}

	public void ClearSpearText() {
		if(MenuStateMachine.instance==null || MenuStateMachine.instance.notInTut()) {
			harpoonAwardMessage.text = extraHarpoonThreshold + " pt throw to gain harpoon";
		}
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
		if(MenuStateMachine.instance && MenuStateMachine.instance.notInTut()) {
			spearsLeftText.text = "Harpoon" + (spearsLeft == 1 ? "" : "s") + ": " + spearsLeft;
		} else {
			spearsLeftText.text = "Tutorial Step " + (int)MenuStateMachine.instance.tutStep;
		}
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
			endScoreText.text = "congrats you earned " + totalScore;
			FMODUnity.RuntimeManager.PlayOneShot("event:/game_over");
		} else {
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
			if(MenuStateMachine.instance.notInTut()) {
				spearsLeft--;
			}
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
		Vector3 textPos = enemyScored.transform.position;

		FishMoverBasic fmbScript = enemyScored.GetComponent<FishMoverBasic>();
		HarpoonablePassingMonster hpmScript = enemyScored.GetComponent<HarpoonablePassingMonster>();

		int scoreAdded = (scoreAmt == -1 ? 
			(fmbScript ? fmbScript.scoreValue : hpmScript.scoreValue ) : 
			scoreAmt);

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
				spearsLeft++;
				if(MenuStateMachine.instance.notInTut()) {
					harpoonAwardMessage.text = "You earned an extra harpoon!";
				}
				extraHarpoonEarnedSinceLastThrow = true;
				UpdateSpearCount();
			}
		}

		GameObject scoreGO = GameObject.Instantiate(scorePopPrefab);
		Text textScript = scoreGO.GetComponent<Text>();
		textScript.text = (scoreAdded > 0 ? "+" : "") + scoreAdded;

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
