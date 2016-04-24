using UnityEngine;
using System.Collections;

public class HarpoonablePassingMonster : MonoBehaviour {
	public MeshRenderer preStabbedFish;
	public MeshRenderer postStabbedFish;
	public int scoreValue = 0;

	public runLaps rlScript;

	public bool stopsHarpoon = true;

	private bool isDead = false;

	// Use this for initialization
	void Start () {
		rlScript = GetComponent<runLaps>();
		showShocked(false);
	}

	void showShocked(bool isShocked) {
		preStabbedFish.enabled = (isShocked == false);
		postStabbedFish.enabled = isShocked;
		isDead = isShocked;
		rlScript.enabled = (isDead == false);
	}

	public void Die () {
		if(isDead == false) {
			showShocked(true);
			StartCoroutine( comeBackToLife() );
		}
	}

	IEnumerator comeBackToLife() {
		yield return new WaitForSeconds(2.5f);
		showShocked(false);
	}

	public bool IsAlive() {
		return (isDead == false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
