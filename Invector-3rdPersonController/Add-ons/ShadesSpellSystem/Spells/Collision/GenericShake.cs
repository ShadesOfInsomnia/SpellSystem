using UnityEngine;
using System.Collections;
using System;

public class GenericShake : MonoBehaviour {

	[NonSerialized] public bool isShaking = false;

	public void ApplyShake (Vector2 noise) {
		Vector3 newPosition = transform.localPosition;
		newPosition.x = noise.x;
		newPosition.y = noise.y;
		transform.localPosition = newPosition;
	}

	public void Enable () {
		isShaking = true;
	}

	public void Disable () {
		isShaking = false;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
	}
}
