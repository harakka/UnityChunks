using UnityEngine;
using System.Collections;

public class PerlinNoiseGen: MonoBehaviour {

	[Range(0.1f, 2f)]
	public float PerlinAmplitude = 0.5f;
	[Range(0.01f, 5f)]
	//public float PerlinFrequency = 2f;
	public float PerlinFrequency = 1f/3f;
	[Range(1, 10)]
	public int PerlinOctaves = 4;
	[Range(0f, 5f)]
	public float PerlinLacunarity = 2f;
	[Range(0f, 5f)]
	public float PerlinGain = 0.5f;

	public float PerlinNoise (float x, float y){
		return PerlinNoise(x,y,0);
	}

	public float PerlinNoise (float x, float y, float t){
		// Using https://code.google.com/p/fractalterraingeneration/wiki/Fractional_Brownian_Motion
		var total = 0f;
		var frequency = PerlinFrequency;
		var amplitude = PerlinAmplitude;
		
		for (int i = 0; i < PerlinOctaves; i++) {
			total += Mathf.PerlinNoise((x)*frequency, (y+t*0.3f)*frequency)*amplitude;
			frequency *= PerlinLacunarity;
			amplitude *= PerlinGain;
			
		}
		return total;
	}
}
