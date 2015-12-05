using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : MonoBehaviour {
	public static ScoreManager instance;
	public GameObject scorePopPrefab;

	public Text totalScoreText;
	public Text lastThrowScoreText;

	public HarpoonDrag lastThrownSpear;

	private int totalScore;
	private int lastThrowScore;

	private Canvas uiCanvas;

	public void NewSpearThrown(HarpoonDrag newOne) {
		lastThrownSpear = newOne;
		lastThrowScoreText.text = ""+lastThrowScore;
		lastThrowScore = 0;
	}
	
	public void ScorePop(FishMoverBasic fmbScored, HarpoonDrag thrownSpear) {
		Vector3 textPos = fmbScored.transform.position;
		int scoreAdded = fmbScored.scoreValue;

		totalScore += scoreAdded;
		totalScoreText.text = ""+totalScore+"   ";

		Debug.Log(thrownSpear.name);
		Debug.Log(lastThrownSpear.name);
		if(thrownSpear == lastThrownSpear) {
			lastThrowScore += scoreAdded;
			lastThrowScoreText.text = ""+lastThrowScore;
		}

		GameObject scoreGO = GameObject.Instantiate(scorePopPrefab);
		Text textScript = scoreGO.GetComponent<Text>();
		textScript.text = "+" + scoreAdded;
		
		scoreGO.transform.parent = uiCanvas.transform;
		
		textPos.z = Camera.main.transform.position.z + 0.35f;
		scoreGO.transform.position = textPos;
		scoreGO.transform.localScale = Vector3.one * 0.8f;
	}
	
	void Awake () {
		if(instance) {
			Destroy(instance);
		}
		instance = this;
		
		uiCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
	}
}
