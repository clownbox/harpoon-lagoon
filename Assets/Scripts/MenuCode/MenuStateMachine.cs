﻿using UnityEngine;
using System.Collections;

public class MenuStateMachine : MonoBehaviour {
	public GameObject firstScreen;

	public GameObject[] blockAction;
	public GameObject[] allowInput;

	public GameObject[] screens;
	public static MenuStateMachine instance;

	public GameObject[] beasts;

	public GameObject inGameUI;

	private bool readyToAdvance = false;

	private bool gameCenterWorking = false;

	public enum TUTORIAL_PHASE
	{
		NormalPlay,
		SteerBoat,
		HoldToAim,
		ReleaseToThrow,
		CancelThrow,
		SpearFish,
		SpearThree,
		Blowfish,
		Monsters, 
		Shark, 
		TutorialDone
	}

	public enum ACHIEVEMENT_ENUM // these should match Achievement IDs in iTunes Connect
	{
		bigToSmall,
		smallToBig,
		matchThree,
		dayTen,
		dayTwenty,
		dayThirty,
		turtleSoup,
		savingThrow
	}

	public void DidAchivement(ACHIEVEMENT_ENUM thisAchievement, float totalProgressAmt) {
		string achievementId = ""+thisAchievement;

		// note: GameCenterManager.getAchievementProgress(thisAchievement) gets last progress

		bool isCompleteNotification = (totalProgressAmt >= 100.0f); 
		GameCenterManager.SubmitAchievement(totalProgressAmt, achievementId, 
			false // Game Center bug workaround
			);
		if(isCompleteNotification) {
			Debug.Log("Achievement Earned: " + achievementId);
			GameCenterManager.ShowGmaeKitNotification("Achievement Earned", achievementId);
		}
		// note: GameCenterManager.resetAchievements(); // will wipe progress
	}

	public TUTORIAL_PHASE tutStep = TUTORIAL_PHASE.NormalPlay;

	public void AllowBeasts(bool haveThem) {
		for(int i=0; i<beasts.Length;i++) {
			beasts[i].SetActive(haveThem);
		}
	}

	public void NextStep(bool instantAdvance = false) {
		readyToAdvance = true;
		if(instantAdvance) {
			HarpoonRetractedConsiderNextTutStep();
		}
	}

	public void HarpoonRetractedConsiderNextTutStep() {
		if(readyToAdvance == false) {
			return;
		}
		readyToAdvance = false;
		if(tutStep == TUTORIAL_PHASE.NormalPlay) {
			AllowBeasts(true);
			return;
		}

		string str = UnityEngine.StackTraceUtility.ExtractStackTrace ();
		Debug.Log(str);

		tutStep = (TUTORIAL_PHASE)( (int)tutStep+1 );
		if(tutStep == TUTORIAL_PHASE.TutorialDone) {
			ScoreManager.instance.EndOfTutorialMessage();
			FishSpawnInfinite.instance.RemoveAll();
		} else {
			FishSpawnInfinite.instance.UpdateText();

			AllowBeasts( (int)tutStep >= (int)TUTORIAL_PHASE.Monsters );

			FishSpawnInfinite.instance.RemoveAll();

			switch(tutStep) {
			case TUTORIAL_PHASE.SpearFish:
				FishSpawnInfinite.instance.AddOneFish();
				break;
			case TUTORIAL_PHASE.SpearThree:
				FishSpawnInfinite.instance.whichFishSeq = 0;
				FishSpawnInfinite.instance.AddOneFish(1);
				FishSpawnInfinite.instance.AddOneFish(2);
				FishSpawnInfinite.instance.AddOneFish(3);
				break;
			case TUTORIAL_PHASE.Blowfish:
				// FishSpawnInfinite.instance.NextLevel();
				FishSpawnInfinite.instance.RemoveAll();
				FishSpawnInfinite.instance.whichFishSeq = 0;
				FishSpawnInfinite.instance.AddOneFish(4);
				FishSpawnInfinite.instance.AddOneFish(4);
				FishSpawnInfinite.instance.AddOneFish(4);
				break;
			case TUTORIAL_PHASE.Shark:
				FishSpawnInfinite.instance.RemoveAll();
				FishSpawnInfinite.instance.whichFishSeq = 0;
				FishSpawnInfinite.instance.AddOneFish(1);
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
			return "Release under water to throw";
		case TUTORIAL_PHASE.CancelThrow:
			return "Aim but release above water to cancel";
		case TUTORIAL_PHASE.SpearFish:
			return "Spear the fish!";
		case TUTORIAL_PHASE.SpearThree:
			return "Hit up to 3 fish at once!";
		case TUTORIAL_PHASE.Blowfish:
			return "Your spear can't go past a blowfish!";
		case TUTORIAL_PHASE.Monsters:
			return "Turtles block throws, octopus costs points!";
		case TUTORIAL_PHASE.Shark:
			return "A shark will steal if you wait. Scare it off!";
		case TUTORIAL_PHASE.TutorialDone:
		default:
			return "";
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

	public void ShowGameCenterIfAbleOtherwiseDisplayThisPanel(GameObject showThisOne) {
		if(gameCenterWorking) {
			GameCenterManager.ShowAchievements();
		} else {
			AllMenusOffExcept(showThisOne);
		}
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

	public void SetupTutorial(int levSequence) {
		bool useTut = (levSequence == -1);

		if(useTut) {
			tutStep = (TUTORIAL_PHASE)( (int)TUTORIAL_PHASE.NormalPlay+1 );
			FishSpawnInfinite.instance.RemoveAll();
			AllowBeasts(false);
			Debug.Log("Removing Beast and all fish");
		} else {
			tutStep = TUTORIAL_PHASE.NormalPlay;
			AllowBeasts(true);
			FishSpawnInfinite.instance.whichFishSeq = levSequence;
			FishSpawnInfinite.instance.RemoveAll();
			FishSpawnInfinite.instance.SpawnForLevel();
			ScoreManager.instance.ResetScore();
		}
		FishSpawnInfinite.instance.UpdateText();
		ScoreManager.instance.UpdateSpearCount();
		AllMenusOffExcept(inGameUI);
	}

	void Awake() {
		instance = this;
	}

	// Use this for initialization
	void Start () {
		AllMenusOffExcept(firstScreen);
		GameCenterManager.OnAuthFinished += OnAuthFinished;
		GameCenterManager.OnAchievementsLoaded += OnAchievementsLoaded;
		GameCenterManager.Init();
	}

	void OnAchievementsLoaded(ISN_Result result) {

		if(result.IsSucceeded) {
			Debug.Log ("Achievemnts was loaded from IOS Game Center");

			foreach(GK_AchievementTemplate tpl in GameCenterManager.Achievements) {
				Debug.Log (tpl.Id + ":  " + tpl.Progress);
			}
		}
	}

	void OnAuthFinished (ISN_Result res) {
		if (res.IsSucceeded) {
			IOSNativePopUpManager.showMessage("Player Authored ", "ID: " + GameCenterManager.Player.Id + "\n" + "Alias: " + GameCenterManager.Player.Alias);
			Debug.Log("Player logged in: " + GameCenterManager.Player.DisplayName);
			gameCenterWorking = true;
		} else {
			IOSNativePopUpManager.showMessage("Game Center ", "Player auth failed");
		}
	}
}
