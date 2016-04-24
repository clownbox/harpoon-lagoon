using UnityEngine;
using System.Collections;

public class HarpoonablePassingMonster : MonoBehaviour {
	public MeshRenderer preStabbedFish;
	public MeshRenderer postStabbedFish;

	private bool isDead = false;

	// Use this for initialization
	void Start () {
		preStabbedFish.enabled = true;
		postStabbedFish.enabled = false;
	}

	public void Die () {
		if(isDead == false) {
			isDead = true;
			preStabbedFish.enabled = false;
			postStabbedFish.enabled = true;
		}
	}

	public bool IsAlive() {
		return (isDead == false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
