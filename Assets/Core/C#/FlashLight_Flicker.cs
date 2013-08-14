using UnityEngine;
using System.Collections;

public class FlashLight_Flicker : MonoBehaviour {

	public int flickerFrequency = 10;
	public float minBrightness = 0.0f;
	public float maxBrightness = 8.0f;
	private int flicker = 0;
	private float totalBrightness;
	
	void Awake () {
		maxBrightness = Mathf.Clamp(maxBrightness, 0.0001f, 8);
		minBrightness = Mathf.Clamp(minBrightness, 0, maxBrightness);
	}
	void FixedUpdate () {
		flicker = Random.Range (1, flickerFrequency);
		if (flicker == 1){
			totalBrightness = Random.Range (minBrightness, maxBrightness);
			light.intensity = totalBrightness;
		}
	}
}