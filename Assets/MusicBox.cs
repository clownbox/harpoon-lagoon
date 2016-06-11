using UnityEngine;
using System.Collections;

public class MusicBox : MonoBehaviour {
	private FMOD.Studio.EventInstance songEvt;

	// Use this for initialization
	void Start () {
		songEvt = FMODUnity.RuntimeManager.CreateInstance("event:/music");
		songEvt.start();
	}
}
