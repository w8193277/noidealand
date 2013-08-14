using UnityEngine;
using System.Collections;

public enum enWaveFunctions{
	sinus =0,
	triangle =1,
	square =2,
	sawtooth =3,
	inverted_saw =4,
	noise =5
}

public class FlickeringLight : MonoBehaviour {
	public enWaveFunctions waveFunction= enWaveFunctions.sinus;
	public float offset =0.0f; // constant offset
	public float amplitude = 1.0f; // amplitude of the wave
	public float phase = 0.0f; // start point inside on wave cycle
	public float frequency = 0.5f; // cycle frequency per second
	public bool affectsIntensity = true;
   
	// Keep a copy of the original values
	private float originalIntensity;
 
	// Use this for initialization
	void Start () {
		originalIntensity = GetComponent<Light>().intensity;
	}
	// Update is called once per frame
	void Update () {
		Light light = GetComponent<Light>();
		if (affectsIntensity)
			light.intensity = originalIntensity * EvalWave();
	}
	private float EvalWave(){
		float x = (Time.time + phase)*frequency;
		float y;
		x = x - Mathf.Floor(x); // normalized value (0..1)
		if (waveFunction==enWaveFunctions.sinus){
			y = Mathf.Sin(x*2f*Mathf.PI);
		}
		else if (waveFunction==enWaveFunctions.triangle) {
			if (x < 0.5f)
				y = 4.0f * x - 1.0f;
			else
				y = -4.0f * x + 3.0f;  
		}
		else if (waveFunction==enWaveFunctions.square) {   
			if (x < 0.5f)  
 				 y = 1.0f;
			else   
 				y = -1.0f;   
		}  
		else if (waveFunction==enWaveFunctions.sawtooth) { 
 			 y = x;   
		}  
		else if (waveFunction==enWaveFunctions.inverted_saw) { 
			y = 1.0f - x;  
		}  
		else if (waveFunction==enWaveFunctions.noise) {
			y = Random.value - (Random.value * frequency);
		}  
		else { 
			y = 1.0f;
		}  
	return (y * amplitude) + offset;
	}
}
