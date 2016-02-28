using UnityEngine;
using System.Collections;

public class TrackObj : MonoBehaviour {
	public Transform matchLoc;

	// Update is called once per frame
	void Update () {
		if(matchLoc) {
			transform.position = matchLoc.position;
		}
	}
}
