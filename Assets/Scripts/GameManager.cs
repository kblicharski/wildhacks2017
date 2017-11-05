using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject fire = GameObject.Instantiate(Resources.Load("Prefabs/fire")) as GameObject;
        fire.transform.position = new Vector3(0, 0, 0);
        fire.GetComponent<AudioSource>().volume = 0.5f;
        fire.GetComponent<AudioSource>().Play();
	}

    // Update is called once per frame
    void Update() {
        if (Random.Range(0, 1.0f) < 0.002f) {
            GameObject newZombie = GameObject.Instantiate(Resources.Load("Prefabs/Zombie")) as GameObject;
            float distance = Random.Range(20, 60);
            float theta = Random.Range(0, Mathf.PI * 2.0f);
            newZombie.transform.position = new Vector3(Mathf.Cos(theta) * distance, 0, Mathf.Sin(theta) * distance);
            Rigidbody body = newZombie.AddComponent<Rigidbody>();
            body.mass = 5;
            float scale = 0.1f;
            body.velocity = new Vector3(0, 0, 0) - newZombie.transform.position;
            body.velocity = Vector3.Scale(body.velocity, new Vector3(scale, scale, scale));
            body.useGravity = false;

            newZombie.transform.rotation = Quaternion.Euler(new Vector3(0, 180 + (Mathf.Rad2Deg * Mathf.Atan2(newZombie.transform.position.x, newZombie.transform.position.z)), 0));
            //body.velocity.Scale(new Vector3(scale, scale, scale));
         }

        if (Random.Range(0, 1.0f) < 0.001f) {
            GameObject newSound = GameObject.Instantiate(Resources.Load("Prefabs/random" + (int)Random.Range(1, 5.99f))) as GameObject;
            float distance = Random.Range(0, 20);
            float theta = Random.Range(0, Mathf.PI * 2.0f);
            newSound.transform.position = new Vector3(Mathf.Cos(theta) * distance, 0, Mathf.Sin(theta) * distance);
            newSound.GetComponent<AudioSource>().Play();
        }
	}
}
