using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;

public class RightUpperArm : MonoBehaviour {

	// Tie the upper arm to the myo controlling it in the inspector.
	// Typically, this should be the NON-primary myo.
	public GameObject myo = null;

	// The pose from the last update. This is used to determine if the pose has changed
	// so that actions are only performed upon making them rather than every frame during
	// which they are active.
	private Pose _lastPose = Pose.Unknown;

	private Quaternion _antiYaw = Quaternion.identity;
	private float _referenceRoll = 0.0f;

	// TODO: make this the actual camera position
	private static Vector3 camPos = new Vector3(0, 0, 0);
	private Vector3 standardPosition = new Vector3(camPos.x + 1.5f, camPos.y - 2.0f, camPos.z);

	public float width = 0.8f;
	public float length = 2.0f;

	void Start() {
		Transform childTransform = this.transform.GetChild (0);
		Vector3 newScale = new Vector3 (width, length, width);
		childTransform.localScale = newScale;

		StartCoroutine (updatePosition ());
	}

	// Update is called once per frame
	/*
	void Update () {
		ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo> ();

		// Resetting capabilities
		if (thalmicMyo.pose != _lastPose) {
			_lastPose = thalmicMyo.pose;

			if (thalmicMyo.pose == Pose.FingersSpread) {
				ExtendUnlockAndNotifyUserAction (thalmicMyo);
				RightStorage.shouldUpdate = true;
			}
		}

		if (RightStorage.shouldUpdate) {
			_antiYaw = Quaternion.FromToRotation (
				new Vector3 (myo.transform.forward.x, 0, myo.transform.forward.z),
				new Vector3 (0, 0, 1)
			);

			Vector3 referenceZeroRoll = computeZeroRollVector (myo.transform.forward);
			_referenceRoll = rollFromZero (referenceZeroRoll, myo.transform.forward, myo.transform.up);
		}

		Vector3 zeroRoll = computeZeroRollVector (myo.transform.forward);
		float roll = rollFromZero (zeroRoll, myo.transform.forward, myo.transform.up);
		float relativeRoll = normalizeAngle (roll - _referenceRoll);
		Quaternion antiRoll = Quaternion.AngleAxis (relativeRoll, myo.transform.forward);

		Quaternion rawQuat = _antiYaw * antiRoll * Quaternion.LookRotation (myo.transform.forward);
		Vector3 rawAngles = rawQuat.eulerAngles;
		//transform.rotation = Quaternion.Euler(new Vector3(90 + rawAngles.x, rawAngles.z, rawAngles.y));

		transform.rotation = rawQuat;

		// Next, we update the position to mimic motion about the "shoulder joint".
		// The position is going to be the "standard pose" (representing the arm pointing forward with the shoulder
		// offset from the "head" camera) plus any orientation and offset due to that.
		transform.position = standardPosition + (transform.rotation * Vector3.forward * length);
		//transform.position = standardPosition;

		RightStorage.upperArmEndPosition = transform.position + (transform.rotation * Vector3.forward * length);
	}
	*/

	Vector3 pastData = new Vector3(0, 0, 0);
	Vector3 currentData = new Vector3(0, 0, 0);

	void Update() {
		//transform.rotation = Quaternion.Lerp (Quaternion.Euler (pastData), Quaternion.Euler (currentData), 0.1f);

		float turningRate = 20f;

		transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.Euler (currentData), turningRate * Time.deltaTime);

		Vector3 pastPosition = standardPosition + (Quaternion.Euler(pastData) * Vector3.forward * length);
		Vector3 currentPosition = standardPosition + (Quaternion.Euler(currentData) * Vector3.forward * length);

		transform.position = Vector3.Lerp (pastPosition, currentPosition, turningRate * Time.deltaTime);
	}

	IEnumerator updatePosition() {
		while (true) {
			WWW www = new WWW ("http://10.105.108.52:5000");
			yield return www;
			ServerData data = JsonUtility.FromJson<ServerData> (www.text);
			Debug.Log (www.text);

			if(www != null && www.text != "") {
				Vector3 rightUpperArmData = new Vector3 (
					Mathf.Rad2Deg * -data.Rightupperarm.pitch,
					Mathf.Rad2Deg * -data.Rightupperarm.roll,
					Mathf.Rad2Deg * -data.Rightupperarm.yaw);

				transform.rotation = Quaternion.Euler(rightUpperArmData);
				transform.position = standardPosition + (transform.rotation * Vector3.forward * length);

				// Update data
				this.pastData = this.currentData;
				this.currentData = rightUpperArmData;

				RightStorage.upperArmEndPosition = transform.position + (transform.rotation * Vector3.forward * length);
			}

		}
	}

	Vector3 computeZeroRollVector (Vector3 forward) {
		Vector3 antigravity = Vector3.up;
		Vector3 m = Vector3.Cross (myo.transform.forward, antigravity);
		Vector3 roll = Vector3.Cross (m, myo.transform.forward);

		return roll.normalized;
	}

	float rollFromZero (Vector3 zeroRoll, Vector3 forward, Vector3 up) {
		float cosine = Vector3.Dot (up, zeroRoll);

		Vector3 cp = Vector3.Cross (up, zeroRoll);
		float directionCosine = Vector3.Dot (forward, cp);
		float sign = directionCosine < 0.0f ? 1.0f : -1.0f;

		// Return the angle of roll (in degrees) from the cosine and the sign.
		return sign * Mathf.Rad2Deg * Mathf.Acos (cosine);
	}

	// Adjust the provided angle to be within a -180 to 180.
	float normalizeAngle (float angle) {
		if (angle > 180.0f) {
			return angle - 360.0f;
		}
		if (angle < -180.0f) {
			return angle + 360.0f;
		}
		return angle;
	}

	// Extend the unlock if ThalmcHub's locking policy is standard, and notifies the given myo that a user action was
	// recognized.
	void ExtendUnlockAndNotifyUserAction (ThalmicMyo myo) {
		ThalmicHub hub = ThalmicHub.instance;

		if (hub.lockingPolicy == LockingPolicy.Standard) {
			myo.Unlock (UnlockType.Timed);
		}

		myo.NotifyUserAction ();
	}
}
