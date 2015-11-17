using UnityEngine;
using System.Collections;

public class SeaBounds : MonoBehaviour {
	public static SeaBounds instance;

	public Transform topRef;
	public Transform leftRef;
	public Transform rightRef;
	public Transform bottomRef;
	public Transform fishLayerRef;
	public Transform marginFromRightEdgeRef;

	private float edgeMargin;

	[HideInInspector]
	public float top;
	[HideInInspector]
	public float left;
	[HideInInspector]
	public float right;
	[HideInInspector]
	public float bottom;
	[HideInInspector]
	public float fishLayerZ;

	// Use this for initialization
	void Awake () {
		if(instance) {
			Destroy(instance);
		}
		fishLayerZ = fishLayerRef.position.z;

		edgeMargin = rightRef.position.x-marginFromRightEdgeRef.transform.position.x;

		top = topRef.position.y-edgeMargin;
		left = leftRef.position.x+edgeMargin;
		right = rightRef.position.x-edgeMargin;
		bottom = bottomRef.position.y+edgeMargin;

		instance = this;
	}

	public Vector3 randPos() {
		Vector3 toRet = Vector3.zero;
		toRet.x = Random.Range(left, right);
		toRet.y = Random.Range(bottom, top);
		toRet.z = fishLayerZ;
		return toRet;
	}

	public Vector3 constrain(Vector3 rawVect) {
		rawVect.x = Mathf.Clamp(rawVect.x,left,right);
		rawVect.y = Mathf.Clamp(rawVect.y,bottom, top);
		return rawVect;
	}

	// using margin going double in opposite direction so giving wiggle room to get off screen
	public bool outOfBounds(Vector3 someVect) {
		return (someVect.x > right+edgeMargin*2 ||
		        someVect.x < left-edgeMargin*2 ||
		        someVect.y > top+edgeMargin*2 ||
		        someVect.y < bottom-edgeMargin*2);
	}

	public Vector3 randPosWithinMinMaxRange(Vector3 startFrom, float minDist, float maxDist) {
		// this approaches bias motion away from edges where boundaries affect motion
		Vector3 randGoal = randPos();
		Vector3 normToward = (randGoal - startFrom).normalized;
		Vector3 inRange = startFrom+normToward*Random.Range(minDist,maxDist);
		return constrain(inRange);
	}
}
