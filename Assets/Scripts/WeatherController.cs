using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[Serializable]
public class WeatherImpacts
{
	public WeatherController.WEATHER_MODE weatherKind; // note: define in same order as WEATHER_MODE
	public float waveIntensity;
	public Color skyTint;
	public GameObject cloudTypeSet;

	public float fishSprintDelayMult;
	public float fishSprintDistMult;
	public float fishDriftMult;
}

public class WeatherController : MonoBehaviour {
	public WeatherImpacts[] weatherDefs; // note: define in same order as WEATHER_MODE

	public enum WEATHER_MODE
	{
		Nice,
		Choppy,
		Rainy,
		Storm,
		NotInitializedYet
	};
	public WEATHER_MODE weatherInteraction = WEATHER_MODE.Nice; 
	WEATHER_MODE wasTI = WEATHER_MODE.NotInitializedYet; // to detect change from inspector or outside of class

	public static float weatherSprintDelayMult;
	public static float weatherSprintDistMult;
	public static float weatherDriftMult;

	public BobInWater playerBoat;
	float playerDefBobDampen;
	public BobInWater waterBob;
	float waterDefBobDampen;

	public Text cycleInteractionText;

	public void CycleInteraction() {
		weatherInteraction++;
		if((int)weatherInteraction >= (int)WEATHER_MODE.NotInitializedYet) {
			weatherInteraction = (WEATHER_MODE)0;
		}
		enforceWeatherMode();
	}

	// Use this for initialization
	void Start () {
		playerDefBobDampen = playerBoat.dampen;
		waterDefBobDampen = waterBob.dampen;

		enforceWeatherMode();
	}

	void enforceWeatherMode() {
		if(wasTI != weatherInteraction) {
			for(int i = 0; i < weatherDefs.Length; i++) {
				weatherDefs[i].cloudTypeSet.SetActive(false);
			}
			cycleInteractionText.text = ""+weatherInteraction;
			WeatherImpacts weatherEffectNow = weatherDefs[(int)weatherInteraction];
			Camera.main.backgroundColor = weatherEffectNow.skyTint;
			playerBoat.dampen = playerDefBobDampen * weatherEffectNow.waveIntensity;
			waterBob.dampen = waterDefBobDampen * weatherEffectNow.waveIntensity;
			weatherEffectNow.cloudTypeSet.SetActive(true);

			weatherSprintDelayMult = weatherEffectNow.fishSprintDelayMult;
			weatherSprintDistMult = weatherEffectNow.fishSprintDistMult;
			weatherDriftMult = weatherEffectNow.fishDriftMult;

			wasTI = weatherInteraction;
		}
	}
	
	// Update is called once per frame
	/* void Update () {
	} */
}
