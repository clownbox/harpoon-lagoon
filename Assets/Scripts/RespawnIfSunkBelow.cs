using UnityEngine;
using System.Collections;

public class RespawnIfSunkBelow : MonoBehaviour {
	public GameObject prefabOfMe;
	// Update is called once per frame
	void Update () {
		if(SeaBounds.instance.outOfBounds(transform.position)) {
			GameObject.Instantiate(prefabOfMe, SeaBounds.instance.randPos(), Quaternion.identity);
			Destroy(gameObject);
		}
	}
}
