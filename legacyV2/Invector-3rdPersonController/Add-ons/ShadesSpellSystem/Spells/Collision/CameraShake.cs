using UnityEngine;
using System.Collections;
using System;

public class CameraShake : GenericShake {

	public float linearIntensity = 0.25f;
	public float angularIntensity = 5f;

	private bool angularShaking = true;

	void Update () {
		if (isShaking) {
			LinearShaking ();
			if (angularShaking)
				AngularShaking ();
		}
	}

	private void LinearShaking () {
		Vector2 shake = UnityEngine.Random.insideUnitCircle * linearIntensity;
		ApplyShake (shake);
	}

	private void AngularShaking () {
		float shake = UnityEngine.Random.Range (-angularIntensity, angularIntensity);
		transform.localRotation = Quaternion.Euler (0f, 0f, shake);
	}

	public void SetAngularShaking(bool state) {
		angularShaking = state;
		if (!angularShaking)
			transform.localRotation = Quaternion.identity;
	}
}