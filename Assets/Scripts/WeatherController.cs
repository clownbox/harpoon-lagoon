using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeatherController : MonoBehaviour {
	public enum WEATHER_MODE
	{
		Clear,
		Overcast,
		Rainy,
		Storm,
		Choppy,
		NotInitializedYet
	};
	public WEATHER_MODE weatherInteraction = WEATHER_MODE.Clear; 
	WEATHER_MODE wasTI = WEATHER_MODE.NotInitializedYet; // to detect change from inspector or outside of class

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
		enforceWeatherMode();
	}

	void enforceWeatherMode() {
		if(wasTI != weatherInteraction) {
			cycleInteractionText.text = ""+weatherInteraction;
			wasTI = weatherInteraction;
		}
	}
	
	// Update is called once per frame
	/* void Update () {
	} */
}
