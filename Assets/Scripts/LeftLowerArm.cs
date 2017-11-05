using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;

public class LeftLowerArm : MonoBehaviour {

	// Tie the upper arm to the myo controlling it in the inspector.
	// Typically, this should be the NON-primary myo.
	public GameObject myo = null;

	// The pose from the last update. This is used to determine if the pose has changed
	// so that actions are only performed upon making them rather than every frame during
	// which they are active.
	private Pose _lastPose = Pose.Unknown;

	// The "default" orientation. When reorienting, this will be updated to the current orientation.
	// This is meant to be the "forward" orientation.
	//private Vector3 referenceOrientation = Vector3.forward;

	private Quaternion _antiYaw = Quaternion.identity;
	private float _referenceRoll = 0.0f;

	private Vector3 standardPosition;

	public float width = 0.6f;
	public float length = 2.0f;

	// Use this for initialization
	void Start() {
		Transform childTransform = this.transform.GetChild (0);
		Vector3 newScale = new Vector3 (width, length, width);
		childTransform.localScale = newScale;
	}

	// Update is called once per frame
	void Update () {
		/*
		// Get the myo associated with this GameObject
		ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo> ();

		// Reset orientation if requested
		if (thalmicMyo.pose != _lastPose) {
			_lastPose = thalmicMyo.pose;

			if (thalmicMyo.pose == Pose.FingersSpread) {
				referenceOrientation = thalmicMyo.transform.rotation.eulerAngles;
				ExtendUnlockAndNotifyUserAction (thalmicMyo);
				Debug.Log ("Resetting orientation!");
			}
		}

		Vector3 myoTransform = thalmicMyo.transform.rotation.eulerAngles;	// Raw transform from myo data
		Vector3 normalizedTransform = myoTransform - referenceOrientation;	// Normalized to the reference orientation
		//Debug.Log (normalizedTransform);

		Quaternion x = Quaternion.AngleAxis (90 + normalizedTransform.x, Vector3.right); 	// Left-right
		Quaternion y = Quaternion.AngleAxis (normalizedTransform.y, Vector3.back); 			// Up-down
		Quaternion z = Quaternion.AngleAxis (normalizedTransform.z, Vector3.up); 			// Rotation (twisting arm)

		// Set the transformation to be equal to the myo's data.
		transform.rotation = x * y * z;
		*/

		ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo> ();

		// Resetting capabilities
		LeftStorage.shouldUpdate = false;
		if (thalmicMyo.pose != _lastPose) {
			_lastPose = thalmicMyo.pose;

			if (thalmicMyo.pose == Pose.FingersSpread) {
				ExtendUnlockAndNotifyUserAction (thalmicMyo);
				LeftStorage.shouldUpdate = true;
			}
		}
			
		if (LeftStorage.shouldUpdate) {
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
		transform.rotation = _antiYaw * antiRoll * Quaternion.LookRotation (myo.transform.forward);

		standardPosition = LeftStorage.upperArmEndPosition;
		transform.position = standardPosition + (transform.rotation * Vector3.forward * length);

		//StartCoroutine (updatePosition());
	}

	/*
	IEnumerator updatePosition() {
		while (true) {
			WWW www = new WWW ("http://localhost:5000");
			yield return www;
			ServerData data = JsonUtility.FromJson<ServerData> (www.text);

			Vector3 leftForearmData = new Vector3 (
				Mathf.Rad2Deg * -data.Leftforearm.pitch,
				Mathf.Rad2Deg * -data.Leftforearm.roll,
				Mathf.Rad2Deg * -data.Leftforearm.yaw);
			//Debug.Log (www.text);

			transform.rotation = Quaternion.Euler(leftForearmData);
			standardPosition = LeftStorage.upperArmEndPosition;
			transform.position = standardPosition + (transform.rotation * Vector3.forward * length);
		}
	}
	*/

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
