﻿using UnityEngine;
using System.Collections;

public class ClickTouchReportToThrow : MonoBehaviour {
	public HarpoonThrower htScript;

	void Update () {
		if (Input.GetButtonDown("Fire1")) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit rhInfo;
			if (Physics.Raycast(ray, out rhInfo))
				htScript.ThrowAt(rhInfo.point);
		}
	}
}
