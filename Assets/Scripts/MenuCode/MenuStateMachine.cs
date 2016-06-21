using UnityEngine;
using System.Collections;

public class MenuStateMachine : MonoBehaviour {
	public GameObject firstScreen;

	public GameObject[] blockAction;
	public GameObject[] allowInput;

	public GameObject[] screens;
	public static MenuStateMachine instance;

	public GameObject[] beasts;

	public GameObject inGameUI;

	public enum TUTORIAL_PHASE
	{
		NormalPlay,
		SteerBoat,
		HoldToAim,
		ReleaseToThrow,
		CancelThrow,
		SpearFish,
		SpearThree,
		ExtraSpear,
		Monsters, 
		TutorialDone
	}

	public TUTORIAL_PHASE tutStep = TUTORIAL_PHASE.NormalPlay;

	public void AllowBeasts(bool haveThem) {
		for(int i=0; i<beasts.Length;i++) {
			beasts[i].SetActive(haveThem);
		}
	}

	public void NextStep() {
		if(tutStep == TUTORIAL_PHASE.NormalPlay) {
			AllowBeasts(true);
			return;
		}

		string str = UnityEngine.StackTraceUtility.ExtractStackTrace ();
		Debug.Log(str);

		tutStep = (TUTORIAL_PHASE)( (int)tutStep+1 );
		if(tutStep == TUTORIAL_PHASE.TutorialDone) {
			ScoreManager.instance.EndOfTutorialMessage();
		} else {
			FishSpawnInfinite.instance.UpdateText();

			AllowBeasts( (int)tutStep >= (int)TUTORIAL_PHASE.Monsters );

			switch(tutStep) {
			case TUTORIAL_PHASE.SpearFish:
				FishSpawnInfinite.instance.AddOneFish();
				break;
			case TUTORIAL_PHASE.SpearThree:
				FishSpawnInfinite.instance.AddOneFish();
				FishSpawnInfinite.instance.AddOneFish();
				FishSpawnInfinite.instance.AddOneFish();
				break;
			}

		}
	}

	public bool notInTut() {
		return (tutStep == TUTORIAL_PHASE.NormalPlay);
	}

	public string tutStepLabel() {
		switch(tutStep) {
		case TUTORIAL_PHASE.SteerBoat:
			return "Tap above water to steer";
		case TUTORIAL_PHASE.HoldToAim:
			return "Hold finger on water to aim";
		case TUTORIAL_PHASE.ReleaseToThrow:
			return "Release over water to throw";
		case TUTORIAL_PHASE.CancelThrow:
			return "Aim but release above water to cancel";
		case TUTORIAL_PHASE.SpearFish:
			return "Spear the fish!";
		case TUTORIAL_PHASE.SpearThree:
			return "Hit up to 3 fish at once!";
		case TUTORIAL_PHASE.ExtraSpear:
			return "Get "+ScoreManager.instance.extraHarpoonThreshold+"+ pt in a throw for an extra spear!";
		case TUTORIAL_PHASE.Monsters:
			return "Turtles block spears, sharks eat fish!";
		case TUTORIAL_PHASE.TutorialDone:
		default:
			return "ERROR end";
		}
	}

	// caching to avoid need to compare state lists each frame
	private bool actionAllowed;
	private bool inputAllowed;
	private bool timeForInGameMusic;

	private bool blockArrayTest(GameObject[] list) {
		for(int i=0;i<list.Length;i++) {
			if(list[i].activeSelf) {
				return true;
			}
		}
		return false;
	}

	public bool MenuBlocksAction() {
		return actionAllowed==false;
	}

	public bool MenuAllowsInput() {
		return inputAllowed;
	}

	public bool MenusAllowInGameMusic() {
		return timeForInGameMusic;
	}

	IEnumerator DelayedInputAcceptance() {
		// this delay blocks accidental input upon starting or resuming gameplay from menu
		timeForInGameMusic = true;
		yield return new WaitForSeconds(0.1f);
		inputAllowed = true;
	}

	public void AllMenusOffExcept(GameObject showThisOne)
	{
		for(int i=0;i<screens.Length;i++) {
			screens[i].SetActive(screens[i]==showThisOne);
		}
		actionAllowed = (blockArrayTest(blockAction)==false);
		if(blockArrayTest(allowInput)) {
			StartCoroutine( DelayedInputAcceptance() );
		} else {
			inputAllowed = false;
			timeForInGameMusic = false;
		}
		if(actionAllowed) {
			MusicBox.instance.ResumeGameplayWeatherMusic();
			Time.timeScale = 1.0f;
		} else  {
			MusicBox.instance.SetMusicTone(MusicBox.MusicTone.Title);
			Time.timeScale = 0.0f;
		}
	}

	public void SetupTutorial(bool useTut) {
		if(useTut) {
			tutStep = (TUTORIAL_PHASE)( (int)TUTORIAL_PHASE.NormalPlay+1 );
			FishSpawnInfinite.instance.RemoveAll();
			AllowBeasts(false);
		} else {
			tutStep = TUTORIAL_PHASE.NormalPlay;
			AllowBeasts(true);
		}
		FishSpawnInfinite.instance.UpdateText();
		AllMenusOffExcept(inGameUI);
	}

	void Awake() {
		instance = this;
	}

	// Use this for initialization
	void Start () {
		AllMenusOffExcept(firstScreen);
	}
}
