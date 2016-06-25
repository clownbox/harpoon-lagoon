using UnityEngine;
using System.Collections;

public class MusicBox : MonoBehaviour {
	public enum MusicTone {Title = 0,
				Calm = 1,
				Storm = 2};
	private FMOD.Studio.EventInstance songEvt;

	public static MusicBox instance;

	public MusicTone weatherGameplaySong;

	void Awake() {
		instance = this;
		songEvt = FMODUnity.RuntimeManager.CreateInstance("event:/music");
	}

	// Use this for initialization
	void Start () {
		songEvt.start();
	}

	public void ResumeGameplayWeatherMusic() {
		SetMusicTone(weatherGameplaySong);
	}

	public void SetMusicTone(MusicTone newTone) {
		// Debug.Log("Changing music to: " + newTone);

		if(newTone != MusicTone.Title) {
			weatherGameplaySong = newTone;
		}

		if(MenuStateMachine.instance.MenusAllowInGameMusic() == false) {
			newTone = MusicTone.Title;
			// Debug.Log("In-menu, overriding music to: " + newTone);
		}

		switch(newTone) {
		case MusicTone.Title:
			songEvt.setParameterValue("Level", 0.0f);
			break;
		case MusicTone.Calm:
			songEvt.setParameterValue("Level", 1.0f);
			break;
		case MusicTone.Storm:
			songEvt.setParameterValue("Level", 2.0f);
			break;
		}
	}
}
