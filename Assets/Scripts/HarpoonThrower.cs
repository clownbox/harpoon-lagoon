using UnityEngine;
using System.Collections;

public class HarpoonThrower : MonoBehaviour {
	public GameObject harpoonPrefab;

	public void ThrowAt (Vector3 targetPoint) {
		Vector3 throwFrom = transform.position;

		throwFrom.z = SeaBounds.instance.fishLayerZ;
		targetPoint.z = throwFrom.z;

		Quaternion throwRot = Quaternion.LookRotation(targetPoint-throwFrom,Vector3.up);
		throwRot *= Quaternion.AngleAxis(90.0f,Vector3.right);

		GameObject harpoonGO = (GameObject)GameObject.Instantiate(harpoonPrefab,throwFrom,throwRot);
		ScoreManager.instance.NewSpearThrown( harpoonGO.GetComponent<HarpoonDrag>() );
	}
}
