using UnityEngine;
using System.Collections;

public class RespawnIfSunkBelow : MonoBehaviour {
	public GameObject prefabOfMe;
	// Update is called once per frame
	void Update () {
		if(SeaBounds.instance.outOfBounds(transform.position)) {
			GameObject GOFish = (GameObject)GameObject.Instantiate(
				prefabOfMe, SeaBounds.instance.randEdgePos(), Quaternion.identity);
			Collider fishColl = GOFish.GetComponent<Collider>();
			fishColl.enabled = true;
			GOFish.name = "Respanwed " + gameObject.name;
			Destroy(gameObject);
		}
	}
}
