using UnityEngine;
using System.Collections;

public class MenuStateMachine : MonoBehaviour {
	public GameObject firstScreen;

	public GameObject[] blockAction;
	public GameObject[] allowInput;

	public GameObject[] screens;
	public static MenuStateMachine instance;

	// caching to avoid need to compare state lists each frame
	private bool actionAllowed;
	private bool inputAllowed;

	private bool blockArrayTest(GameObject[] list) {
		for(int i=0;i<list.Length;i++) {
			if(list[i].activeSelf) {
				return true;
			}
		}
		return false;
	}

	public bool MenuBlocksAction() {
		return actionAllowed==false;
	}

	public bool MenuAllowsInput() {
		return inputAllowed;
	}

	IEnumerator DelayedInputAcceptance() {
		// this delay blocks accidental input upon starting or resuming gameplay from menu
		yield return new WaitForSeconds(0.1f);
		inputAllowed = true;
	}

	public void AllMenusOffExcept(GameObject showThisOne)
	{
		for(int i=0;i<screens.Length;i++) {
			screens[i].SetActive(screens[i]==showThisOne);
		}
		actionAllowed = (blockArrayTest(blockAction)==false);
		if(blockArrayTest(allowInput)) {
			StartCoroutine( DelayedInputAcceptance() );
		} else {
			inputAllowed = false;
		}
		if(actionAllowed) {
			Time.timeScale = 1.0f;
		} else  {
			Time.timeScale = 0.0f;
		}
	}

	// Use this for initialization
	void Start () {
		instance = this;
		AllMenusOffExcept(firstScreen);
	}
}
