using UnityEngine;
using System.Collections;

public class SeaBounds : MonoBehaviour {
	public static SeaBounds instance;

	public Transform spearTopRef;

	public Transform topRef;
	public Transform leftRef;
	public Transform rightRef;
	public Transform bottomRef;
	public Transform fishLayerRef;
	public Transform marginFromRightEdgeRef;

	private float edgeMargin;

	[HideInInspector]
	public float spearTop;
	[HideInInspector]
	public float top;
	[HideInInspector]
	public float left;
	[HideInInspector]
	public float right;
	[HideInInspector]
	public float middleX;
	[HideInInspector]
	public float centerY;
	[HideInInspector]
	public float bottom;
	[HideInInspector]
	public float fishLayerZ;

	// Use this for initialization
	void Awake () {
		if(instance) {
			Destroy(instance);
		}
		instance = this;

		fishLayerZ = fishLayerRef.position.z;

		edgeMargin = rightRef.position.x-marginFromRightEdgeRef.transform.position.x;

		spearTop = spearTopRef.position.y-edgeMargin;
		top = topRef.position.y-edgeMargin;
		left = leftRef.position.x+edgeMargin;
		right = rightRef.position.x-edgeMargin;
		bottom = bottomRef.position.y+edgeMargin;

		middleX = (left + right)/2;
		centerY = (top + bottom)/2;
	}

	public Vector3 randPos() {
		Vector3 toRet = Vector3.zero;
		toRet.x = Random.Range(left, right);
		toRet.y = Random.Range(bottom, top);
		toRet.z = fishLayerZ;
		return toRet;
	}

	public float leftish() {
		float perc = Random.Range(0.65f,1.0f);
		return left * perc + middleX * (1.0f - perc);
	}
	public float rightish() {
		float perc = Random.Range(0.65f,1.0f);
		return right * perc + middleX * (1.0f - perc);
	}
	public float topish() {
		float perc = Random.Range(0.65f,1.0f);
		return top * perc + centerY * (1.0f - perc);
	}
	public float bottomish() {
		float perc = Random.Range(0.65f,1.0f);
		return bottom * perc + centerY * (1.0f - perc);
	}

	public Vector3 randEdgePos() {
		Vector3 tempRand = randPos();
		if( Random.Range(0.0f,1.0f) < 0.5f) {
			tempRand.x = left;
		} else {
			tempRand.x = right;
		}
		return tempRand;
	}

	public Vector3 randPosBandBias(float depthBiasOdds, float shallowPerc, float deepPerc, bool anyHoriz = false) {
		Vector3 toRet = Vector3.zero;
		if(anyHoriz) {
			toRet.x = Random.Range(left, right);
		} else {
			if(Random.Range(0, 100) < 50) {
				toRet.x = left;
			} else {
				toRet.x = right;
			}
		}
		// toRet.x = Random.Range(left, right);
		if(Random.Range(0.0f, 1.0f) <= depthBiasOdds ) {
			float range = top - bottom;
			toRet.y = bottom + range * (1.0f-Random.Range(shallowPerc,deepPerc));
		} else {
			toRet.y = Random.Range(bottom, top);
		}
		toRet.z = fishLayerZ;
		return toRet;
	}

	public Vector3 constrain(Vector3 rawVect) {
		if(rawVect.x < left) {
			rawVect.x = Random.Range(left, right);
		}
		if(rawVect.x > right) {
			rawVect.x = Random.Range(left, right);
		}
		// clamping as a sanity measure in case goal projected far outside edge
		rawVect.x = Mathf.Clamp(rawVect.x,left,right);

		rawVect.y = Mathf.Clamp(rawVect.y,bottom, top);
		return rawVect;
	}

	public Vector3 constrainTrunc(Vector3 rawVect) {
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

	public bool spearOutOfBounds(Vector3 someVect, HarpoonDrag spearRef) {
		return ( (someVect.x > right+edgeMargin*0.1 && spearRef.motion.x > 0.0f) ||
			(someVect.x < left-edgeMargin*0.1 && spearRef.motion.x < 0.0f) ||
			someVect.y > spearTop+edgeMargin*0.1 ||
			someVect.y < bottom-edgeMargin*0.1);
	}

	public Vector3 randPosWithinMinMaxRange(Vector3 startFrom, float minDist, float maxDist,
	                                        FishMoverBasic fmb = null) {
		// this approaches bias motion away from edges where boundaries affect motion
		Vector3 randGoal = ( fmb == null ? randPos() : 
		                    randPosBandBias(fmb.depthBiasOdds,
		                fmb.shallowPerc,
		                fmb.deepPerc, true) );
		// this next bit helps reduce bunching near edges
		if(Random.Range(0.0f,1.0f) < 0.6f) { // X% of the time, average halfway to center
			float centerX = (left + right) * 0.5f;
			randGoal.x = (randGoal.x + centerX) * 0.5f; // weight toward middle
			float diffX = randGoal.x - centerX;
			randGoal.x -= diffX * 1.5f; // encourage crossing over
		}
		Vector3 normToward = (randGoal - startFrom).normalized;
		Vector3 inRange = startFrom+normToward*Random.Range(minDist,maxDist);
		return constrainTrunc(inRange);
	}
}
