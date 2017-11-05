using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using Pose = Thalmic.Myo.Pose;
using UnlockType = Thalmic.Myo.UnlockType;
using VibrationType = Thalmic.Myo.VibrationType;

public class CustomJoints : MonoBehaviour {

	// Myo game object to connect with.
	// This object must have a ThalmicMyo script attached.
	public GameObject myo = null;

	// The pose from the last update. This is used to determine if the pose has changed
	// so that actions are only performed upon making them rather than every frame during
	// which they are active.
	private Pose _lastPose = Pose.Unknown;

	private Quaternion _antiYaw = Quaternion.identity;
	private float _referenceRoll = 0.0f;

	//private Vector3 referenceOrientation = Vector3.forward;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

		ThalmicMyo thalmicMyo = myo.GetComponent<ThalmicMyo> ();

		// Resetting capabilities
		bool updateReference = false;
		if (thalmicMyo.pose != _lastPose) {
			_lastPose = thalmicMyo.pose;

			if (thalmicMyo.pose == Pose.FingersSpread) {
				ExtendUnlockAndNotifyUserAction (thalmicMyo);
				updateReference = true;
			}
		}


		if (updateReference) {
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
