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
	public float weatherFade = 0.0f;
	public WEATHER_MODE weatherInteraction = WEATHER_MODE.Nice; 

	public static float weatherSprintDelayMult;
	public static float weatherSprintDistMult;
	public static float weatherDriftMult;
	public static float weatherDriftMultSmoothed;

	public BobInWater playerBoat;
	float playerDefBobDampen;
	public BobInWater waterBob;
	float waterDefBobDampen;

	// public Text cycleInteractionText;

	public void CycleInteraction() {
		weatherInteraction++;
		if((int)weatherInteraction >= (int)WEATHER_MODE.NotInitializedYet) {
			weatherInteraction = (WEATHER_MODE)0;
		}
		enforceWeatherMode();
	}

	public void WeatherSliderUpdated(Slider theSlider) {
		weatherFade = theSlider.value;
		weatherInteraction = (WEATHER_MODE)((int)weatherFade);
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
		for(int i = 0; i < weatherDefs.Length; i++) {
			weatherDefs[i].cloudTypeSet.SetActive(false);
		}
		// cycleInteractionText.text = ""+weatherInteraction;
		WeatherImpacts weatherEffectNow = weatherDefs[(int)weatherInteraction];
		if((int)weatherInteraction < (int)(weatherDefs.Length - 1)) {
			WeatherImpacts weatherEffectNext = weatherDefs[(int)(weatherInteraction + 1)];
			float relativeWeight = 1.0f - Mathf.Repeat(weatherFade, 1.0f);
			float otherWeight = 1.0f - relativeWeight;
			Camera.main.backgroundColor = 	weatherEffectNow.skyTint * relativeWeight +
									 		weatherEffectNext.skyTint * otherWeight;
			playerBoat.dampen = playerDefBobDampen * (weatherEffectNow.waveIntensity * relativeWeight +
													weatherEffectNext.waveIntensity * otherWeight);
			waterBob.dampen = waterDefBobDampen * (weatherEffectNow.waveIntensity * relativeWeight +
													weatherEffectNext.waveIntensity * otherWeight);
			weatherDriftMultSmoothed = (weatherEffectNow.fishDriftMult * relativeWeight +
										weatherEffectNext.fishDriftMult * otherWeight);
			weatherEffectNow.cloudTypeSet.SetActive(relativeWeight >= 0.5f);
			weatherEffectNext.cloudTypeSet.SetActive(relativeWeight < 0.5f);
		} else { // no fade, snapped to top end weather case
		
			Camera.main.backgroundColor = weatherEffectNow.skyTint;
			playerBoat.dampen = playerDefBobDampen * weatherEffectNow.waveIntensity;
			waterBob.dampen = waterDefBobDampen * weatherEffectNow.waveIntensity;
			weatherDriftMultSmoothed = weatherEffectNow.fishDriftMult;
			weatherEffectNow.cloudTypeSet.SetActive(true);
		}


		weatherSprintDelayMult = weatherEffectNow.fishSprintDelayMult;
		weatherSprintDistMult = weatherEffectNow.fishSprintDistMult;
		weatherDriftMult = weatherEffectNow.fishDriftMult;
	}
	
	// Update is called once per frame
	/* void Update () {
	} */
}
