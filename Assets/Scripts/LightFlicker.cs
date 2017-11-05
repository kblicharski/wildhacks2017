using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour {

    float randomStart = 0;
    float speed = 5.0f;

	// Use this for initialization
	void Start () {
        randomStart = Random.Range(-1000.0f, 1000.0f);
    }
	
	// Update is called once per frame
	void Update () {
        Light light = this.GetComponent<Light>();

        float val = Mathf.PerlinNoise(randomStart, Time.time * speed) * 1.0f + 2.0f;
        light.intensity = val;
	}
}
