using UnityEngine;
using System.Collections;

public class RandomizeHorizPos : MonoBehaviour {
	Vector3 startPos;
	// Use this for initialization
	void Start () {
		startPos = transform.position;
		StartCoroutine( RandHoriz() );
	}
	
	IEnumerator RandHoriz () {
		while(true) {
			yield return new WaitForSeconds(Random.Range(1.75f, 4.5f));
			Vector3 newPos = SeaBounds.instance.randPos();
			newPos.y = startPos.y;
			newPos.z = startPos.z;
			transform.position = newPos;
		}
	}
}
