using UnityEngine;
using System.Collections;

public class SlerpUpright : MonoBehaviour {
	// Update is called once per frame
	void Update () {
		transform.rotation =
			Quaternion.Slerp( transform.rotation,
			                 Quaternion.identity, Time.deltaTime*3.0f);
	}
}
