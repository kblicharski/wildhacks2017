using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieKillingObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.name.Contains("ombie"))
        {
            GameObject.Destroy(collision.gameObject, 2.0f);
            Debug.Log("Popped a Zombie!");
        }
    }
}
