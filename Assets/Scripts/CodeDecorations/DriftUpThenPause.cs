using UnityEngine;
using System.Collections;

public class DriftUpThenPause : MonoBehaviour {
	void Start () {
		Destroy(gameObject,1.75f);
		Destroy(this,0.75f);
	}
	
	void Update () {
		transform.position += Vector3.up * Time.deltaTime * 0.5f;
	}
}
