using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sperlich.Core {
	public static class Maths {

		public static float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo) {
			return targetFrom + (source - sourceFrom) * (targetTo - targetFrom) / (sourceTo - sourceFrom);
		}
		public static float Remap(int source, float fromMin, float fromMax, float toMin, float toMax) {
			return Remap((float)source, fromMin, fromMax, toMin, toMax);
		}
		public static float LerpUnclamped(float a, float b, float t) {
			return (1 - t) * a + t * b;
		}
		public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value) {
			Vector3 AB = b - a;
			Vector3 AV = value - a;
			return Mathf.Clamp01(Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB));
		}
		public static (Vector3 position, Quaternion rotation) RotateAround(Transform transform, Vector3 center, Vector3 axis, float angle) => RotateAround(transform.position, transform.rotation, center, axis, angle);
		public static (Vector3 position, Quaternion rotation) RotateAround(Vector3 position, Quaternion rotation, Vector3 center, Vector3 axis, float angle) {
			var rot = Quaternion.AngleAxis(angle, axis); // get the desired rotation
			var dir = position - center; // find current direction relative to center
			dir = rot * dir; // rotate the direction

			Vector3 newPos = center + dir; // define new position
			Quaternion newRot = rotation * Quaternion.Inverse(rotation) * rot * rotation;
			return (newPos, newRot);
		}
	}
}